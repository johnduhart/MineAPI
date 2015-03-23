using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using MineAPI.Common.Logging;
using MineAPI.Network.Crypto;
using MineAPI.Network.IO;
using MineAPI.Protocol;
using MineAPI.Protocol.IO;
using MineAPI.Protocol.Packets;
using MineAPI.Protocol.Packets.Login;

namespace MineAPI.Network
{
    public class NetworkEngine
    {
        private static readonly ILog Log = LogProvider.For<NetworkEngine>();

        private readonly ConcurrentQueue<IPacket> _packetQueue = new ConcurrentQueue<IPacket>();
        private readonly MultiValueDictionary<Type, Action<IPacket>> _packetCallbacks = new MultiValueDictionary<Type, Action<IPacket>>();
        private readonly ConcurrentBag<IObserver<IPacket>> _packetObservers = new ConcurrentBag<IObserver<IPacket>>(); 
        private readonly IPacketInfoSource _packetInfoSource = new PacketInfoSource();

        private NetworkState _currentState = NetworkState.None;
        private Thread _networkThread;

        private TcpClient _client;
        private NetworkStream _networkStream;
        private Stream _baseStream;
        private IMinecraftStreamWriter _writer;
        private IMinecraftStreamReader _reader;
        private bool _compressionEnabled;
        private int _compressionThreshold;
        private byte[] _sharedSecret;

        public IObservable<IPacket> PacketStream
        {
            get
            {
                return Observable.Create<IPacket>(observer =>
                {
                    _packetObservers.Add(observer);


                    return () =>
                    {
                        IObserver<IPacket> x;
                        _packetObservers.TryTake(out x);
                    };
                });
            }
        } 

        public void Connect(IPEndPoint endPoint)
        {
            _client = new TcpClient();
            _client.Connect(endPoint);

            _networkStream = _client.GetStream();
            _baseStream = _networkStream;
            _writer = new MinecraftStreamWriter(_baseStream);
            _reader = new MinecraftStreamReader(_baseStream);

            _networkThread = new Thread(NetworkThread);
            _networkThread.Start();
        }

        public void SendPacket(IPacket packet)
        {
            _packetQueue.Enqueue(packet);
        }

        public Task<T> WaitForPacketAsync<T>() where T : IPacket
        {
            var tcs = new TaskCompletionSource<T>();

            Action<IPacket> callback = null;
            callback = packet =>
            {
                tcs.SetResult((T) packet);

                _packetCallbacks.Remove(typeof (T), callback);
            };

            _packetCallbacks.Add(typeof(T), callback);

            return tcs.Task;
        }

        private void NetworkThread()
        {
            while (true)
            {
                if (!_client.Connected)
                    return;

                // Send queued packets
                while (_packetQueue.Count != 0)
                {
                    IPacket packet;
                    if (!_packetQueue.TryDequeue(out packet))
                        continue;

                    WritePacket(packet);

                    CheckStateChange(packet);
                }

                while (_networkStream.DataAvailable)
                {
                    var packet = ReadPacket();

                    if (packet == null)
                        continue;

                    Log.TraceFormat("S->C {0}", packet.GetType().Name);

                    HandlePacket(packet);
                }

                Thread.Yield();
            }
        }

        private void HandlePacket(IPacket packet)
        {
            // Built in state modifiers
            var setCompression = packet as SetCompressionPacket?;
            if (setCompression.HasValue)
            {
                _compressionEnabled = true;
                _compressionThreshold = setCompression.Value.Threshold;

                Log.DebugFormat("Compression enabled. Threshold={0}", _compressionThreshold);
            }

            var encyptionRequest = packet as EncryptionRequestPacket?;
            if (encyptionRequest.HasValue)
            {
                HandleEncryption(encyptionRequest.Value);
            }

            // Call handlers
            IReadOnlyCollection<Action<IPacket>> callbacks;
            if (_packetCallbacks.TryGetValue(packet.GetType(), out callbacks))
            {
                // Make a copy as callbacks might modify the collection
                foreach (Action<IPacket> callback in callbacks.ToList())
                {
                    callback(packet);
                }
            }

            foreach (var observer in _packetObservers)
            {
                observer.OnNext(packet);
            }

            CheckStateChange(packet);
        }

        private void HandleEncryption(EncryptionRequestPacket packet)
        {
            var random = RandomNumberGenerator.Create();
            _sharedSecret = new byte[16];
            random.GetBytes(_sharedSecret);

            if (packet.ServerId != "")
            {
                throw new NotImplementedException("Online mode is not implemented");
            }

            var parser = new AsnKeyParser(packet.PublicKey);
            var key = parser.ParseRSAPublicKey();

            // Encrypt shared secret and verification token
            var crypto = new RSACryptoServiceProvider();
            crypto.ImportParameters(key);
            WritePacket(new EncryptionResponsePacket
            {
                SharedSecret = crypto.Encrypt(_sharedSecret, false),
                VerifyToken = crypto.Encrypt(packet.VerifyToken, false),
            });

            _baseStream = new AesStream(_networkStream, _sharedSecret);
            _writer = new MinecraftStreamWriter(_baseStream);
            _reader = new MinecraftStreamReader(_baseStream);
        }

        private void WritePacket(IPacket packet)
        {
            Log.TraceFormat("C->S {0}", packet.GetType());

            IPacketInfo packetInfo = _packetInfoSource.GetPacketInfo(packet);

            // TODO: Error handling
            if (packetInfo == null)
                return;
            
            var dataStream = new MemoryStream();
            var dataWriter = new MinecraftStreamWriter(dataStream);
            dataWriter.WriteVarInt(packetInfo.Id);
            packetInfo.WritePacketToStream(packet, dataWriter);

            if (_compressionEnabled)
            {
                // Packet ID + Data
                byte[] compressedData;
                int dataLength;

                if (dataStream.Length > _compressionThreshold)
                {
                    // Compress the packet id + data
                    using (var outputStream = new MemoryStream())
                    using (var inputStream = new DeflaterOutputStream(outputStream, new Deflater(0)))
                    {
                        inputStream.Write(dataStream.GetBuffer(), 0, (int) dataStream.Length);
                        inputStream.Close();

                        compressedData = outputStream.ToArray();
                    }

                    dataLength = compressedData.Length;
                }
                else
                {
                    // Not really compressed
                    compressedData = dataStream.ToArray();
                    dataLength = 0;
                }

                int packetLength = dataLength.GetVarIntLength() + compressedData.Length;

                // Packet length, data length, then compressed data
                _writer.WriteVarInt(packetLength);
                _writer.WriteVarInt(dataLength);
                _baseStream.Write(compressedData, 0, compressedData.Length);
            }
            else
            {
                // Write packet length and ID
                _writer.WriteVarInt((int) dataStream.Length);
                //_writer.WriteVarInt(packetInfo.Id);
                _baseStream.Write(dataStream.GetBuffer(), 0, (int)dataStream.Length);
            }
            

            _networkStream.Flush();
        }

        private IPacket ReadPacket()
        {
            byte[] data;
            int packetId;

            if (!_compressionEnabled)
            {
                int length = _reader.ReadVarInt() - 1; // Assume that the packet Id is always one byte
                Debug.Assert(length > 0);

                packetId = _reader.ReadVarInt();
                data = _reader.ReadByteArray(length);
            }
            else
            {
                int packetLength = _reader.ReadVarInt();
                Debug.Assert(packetLength > 0);

                int dataLengthBytes;
                int dataLength = _reader.ReadVarInt(out dataLengthBytes);

                if (dataLength == 0)
                {
                    packetId = _reader.ReadVarInt();

                    data = _reader.ReadByteArray(packetLength - 2);
                }
                else
                {
                    byte[] compressedBytes = _reader.ReadByteArray(packetLength - dataLengthBytes);

                    using (var outputStream = new MemoryStream())
                    using (var inputStream = new InflaterInputStream(new MemoryStream(compressedBytes)))
                    {
                        inputStream.CopyTo(outputStream);
                        byte[] uncompressedBytes = outputStream.ToArray();

                        packetId = uncompressedBytes[0];
                        data = new byte[uncompressedBytes.Length - 1];
                        Buffer.BlockCopy(uncompressedBytes, 1, data, 0, data.Length);
                    }
                }
            }

            return ReadPacket((byte) packetId, data);
        }

        private IPacket ReadPacket(byte packetId, byte[] data)
        {
            // TODO: fuck. direction wont work
            IPacketInfo packetInfo = _packetInfoSource.GetPacketInfo(packetId, PacketDirection.Clientbound, _currentState);

            if (packetInfo == null)
            {
                Log.WarnFormat("S->C Unkown packet 0x{0:X2}. Size: {1} bytes", packetId, data.Length);
                return null;
            }

            IPacket packet;

            using (var memoryStream = new MemoryStream(data))
            using (var reader = new MinecraftStreamReader(memoryStream))
            {
                packet = packetInfo.ReadPacketFromStream(reader);

                if (memoryStream.Position < data.Length)
                    Log.WarnFormat("Packet {0} did not read all available data", packetInfo.GetType().Name);
            }

            return packet;
        }

        private void CheckStateChange(IPacket packet)
        {
            var handshakePacket = packet as IStateChangePacket;
            if (handshakePacket != null)
            {
                NetworkState nextState = handshakePacket.NewState;
                Log.DebugFormat("Changing network state from {0} to {1}", _currentState, nextState);
                _currentState = nextState;
            }
        }
    }
}
