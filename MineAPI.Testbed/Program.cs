using System;
using System.Net;
using System.Reactive.Linq;
using MineAPI.Common;
using MineAPI.Network;
using MineAPI.Protocol;
using MineAPI.Protocol.Packets;
using MineAPI.Protocol.Packets.Login;
using MineAPI.Protocol.Packets.Play;
using MineAPI.Protocol.Packets.Play.Clientbound;
using MineAPI.Protocol.Packets.Play.Serverbound;
using Serilog;

namespace MineAPI.Testbed
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup log
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Log.Information("Hi");

            var network = new NetworkEngine();
            network.Connect(new IPEndPoint(IPAddress.Loopback, 25565));

            /*network.SendPacket(new HandshakePacket
            {
                ProtocolVersion = 47,
                ServerAddress = "localhost",
                ServerPort = 25565,
                NextState = NetworkState.Status,
            });

            network.SendPacket(new StatusRequestPacket());

            var response = network.WaitForPacketAsync<StatusResponsePacket>().Result;

            Console.WriteLine(response.Response);

            network.SendPacket(new StatusTimePacket
            {
                Time = DateTime.Now.Ticks,
            });

            var pingResponse = network.WaitForPacketAsync<StatusTimePacket>().Result;

            Console.WriteLine(pingResponse.Time);

            Console.ReadLine();*/

            IObservable<IPacket> packetStream = network.PacketStream;


            Vector3? currentPosition = null;

            packetStream
                .OfType<KeepAlivePacket>()
                .Subscribe(k => network.SendPacket(new KeepAlivePacket {Id = k.Id}));

            packetStream
                .OfType<PlayerPositionAndLookPacket>()
                .Subscribe(p =>
                {
                    Console.Title = "Position: " + p.Position;
                    currentPosition = p.Position;
                    network.SendPacket(new PlayerPositionPacket
                    {
                        Position = p.Position,
                        OnGround = true,
                    });
                });

            packetStream
                .OfType<UpdateHealthPacket>()
                .Where(p => p.Health < 1)
                .Subscribe(p =>
                {
                    Log.Fatal("Died! Respawning...");

                    network.SendPacket(new ClientStatusPacket {Action = ClientStatusAction.PerformRespawn});
                });

            packetStream
                .OfType<UpdateHealthPacket>()
                .Subscribe(p => Log.Error("Health: {Health}", p.Health));

            Observable.Interval(TimeSpan.FromMilliseconds(50))
                .Subscribe(_ =>
                {
                    if (currentPosition.HasValue)
                        network.SendPacket(new PlayerPositionPacket {Position = currentPosition.Value, OnGround = true});
                });

            network.SendPacket(new HandshakePacket
            {
                ProtocolVersion = 47,
                ServerAddress = "localhost",
                ServerPort = 25565,
                NextState = NetworkState.Login
            });
            network.SendPacket(new LoginStartPacket {Username = "bot"});

            Console.ReadLine();
        }
    }
}
