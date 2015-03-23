using System;
using System.Net;
using System.Reactive.Linq;
using MineAPI.Network;
using MineAPI.Protocol;
using MineAPI.Protocol.Packets;
using MineAPI.Protocol.Packets.Login;
using MineAPI.Protocol.Packets.Play;
using Serilog;

namespace MineAPI.Testbed
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup log
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
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

            network.PacketStream
                .OfType<KeepAlivePacket>()
                .Subscribe(k => network.SendPacket(new KeepAlivePacket {Id = k.Id}));

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
