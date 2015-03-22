using System;
using System.IO;
using System.Text;

namespace MineAPI.Protocol.IO
{
    public class MinecraftStreamReader : IMinecraftStreamReader, IDisposable
    {
        private readonly Stream _baseStream;

        public MinecraftStreamReader(Stream baseStream)
        {
            _baseStream = baseStream;
        }

        public MinecraftStreamReader(byte[] data)
        {
            _baseStream = new MemoryStream(data);
        }

        public void Dispose()
        {
            _baseStream.Dispose();
        }

        public bool ReadBool()
        {
            return ReadUByte() != 0;
        }

        public sbyte ReadByte()
        {
            return (sbyte) _baseStream.ReadByte();
        }

        public byte ReadUByte()
        {
            return (byte) _baseStream.ReadByte();
        }

        public byte[] ReadByteArray(int length)
        {
            var bytes = new byte[length];

            if (length == 0)
                return bytes;

            int bytesRemaining = length;

            do
            {
                bytesRemaining -= _baseStream.Read(bytes, length - bytesRemaining, bytesRemaining);
            } while (bytesRemaining > 0);

            return bytes;
        }

        public short ReadShort()
        {
            var bytes = ReadByteArray(2);
            Array.Reverse(bytes);

            return BitConverter.ToInt16(bytes, 0);
        }

        public ushort ReadUShort()
        {
            return (ushort)((ReadByte() << 8) | ReadByte());
        }

        public int ReadInt()
        {
            var bytes = ReadByteArray(4);
            Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }

        public uint ReadUInt()
        {
            return (uint)(
                (ReadUShort() << 24) |
                (ReadUShort() << 16) |
                (ReadUShort() << 8) |
                 ReadUShort());
        }

        public long ReadLong()
        {
            var bytes = ReadByteArray(8);
            Array.Reverse(bytes);

            return BitConverter.ToInt64(bytes, 0);
        }

        public ulong ReadULong()
        {
            return unchecked(
                   ((ulong)ReadUShort() << 56) |
                   ((ulong)ReadUShort() << 48) |
                   ((ulong)ReadUShort() << 40) |
                   ((ulong)ReadUShort() << 32) |
                   ((ulong)ReadUShort() << 24) |
                   ((ulong)ReadUShort() << 16) |
                   ((ulong)ReadUShort() << 8) |
                    (ulong)ReadUShort());
        }

        public float ReadFloat()
        {
            var bytes = ReadByteArray(4);
            Array.Reverse(bytes);

            return BitConverter.ToSingle(bytes, 0);
        }

        public double ReadDouble()
        {
            var bytes = ReadByteArray(8);
            Array.Reverse(bytes);

            return BitConverter.ToDouble(bytes, 0);
        }

        public int ReadVarInt()
        {
            int length;
            return ReadVarInt(out length);
        }

        public int ReadVarInt(out int length)
        {
            var result = 0;
            length = 0;

            while (true)
            {
                var current = ReadByte();
                result |= (current & 0x7F) << length++ * 7;

                if (length > 6)
                    throw new InvalidDataException("Invalid varint: Too long.");

                if ((current & 0x80) != 0x80)
                    break;
            }

            return result;
        }

        public string ReadString()
        {
            var length = ReadVarInt();
            var stringBytes = ReadByteArray(length);

            return Encoding.UTF8.GetString(stringBytes);
        }
    }
}