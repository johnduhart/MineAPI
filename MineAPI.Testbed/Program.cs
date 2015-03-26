using System;
using System.Linq;
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
                .MinimumLevel.Information()
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

            const string playerOfInterest = "compwhizii";
            PlayerUuid? interestUuid = null;
            int? interestEid = null;

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

                    Observable.Timer(TimeSpan.FromSeconds(5))
                        .Subscribe(
                            _ => network.SendPacket(new ClientStatusPacket {Action = ClientStatusAction.PerformRespawn}));
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

            packetStream
                .OfType<EntityVelocityPacket>()
                .Subscribe(p => Log.Warning("Velocity: EID: {0}, X: {1}, Y: {2}, Z: {3}",
                    p.EntityId, p.VelocityX, p.VelocityY, p.VelocityZ));

            packetStream
                .OfType<EntityRelativeMovePacket>()
                .Subscribe(p => Log.Warning("RelativeMove: EID: {0}, X: {1}, Y: {2}, Z: {3}",
                    p.EntityId, p.DeltaPosition.X, p.DeltaPosition.Y, p.DeltaPosition.Z));

            packetStream
                .OfType<EntityLookAndRelativeMovePacket>()
                .Subscribe(p => Log.Warning("RelativeMove: EID: {0}, X: {1}, Y: {2}, Z: {3}",
                    p.EntityId, p.DeltaPosition.X, p.DeltaPosition.Y, p.DeltaPosition.Z));

            packetStream
                .OfType<EntityTeleportPacket>()
                .Subscribe(p => Log.Warning("Teleport: EID: {0}, X: {1}, Y: {2}, Z: {3}",
                    p.EntityId, p.Position.X, p.Position.Y, p.Position.Z));

            packetStream
                .OfType<PlayerListItemPacket>()
                .Where(p => p.Action == PlayerListAction.AddPlayer)
                .SelectMany(p => p.PlayerList.Cast<PlayerListItemActionAddPlayer>())
                .Subscribe(p =>
                {
                    Log.Information("Player added. UUID: {UUID} Username: {Username} Ping: {Ping}", p.Uuid, p.Name, p.Ping);

                    if (p.Name == playerOfInterest)
                    {
                        Log.Error("Player is of interest!");
                        interestUuid = p.Uuid;
                    }
                });

            packetStream
                .OfType<PlayerListItemPacket>()
                .Where(p => p.Action == PlayerListAction.RemovePlayer)
                .SelectMany(p => p.PlayerList.Cast<PlayerListItemActionRemovePlayer>())
                .Subscribe(p =>
                {
                    Log.Information("Player removed. UUID: {UUID}", p.Uuid);
                });

            packetStream
                .OfType<SpawnPlayerPacket>()
                .Subscribe(p => Log.Information("Player spawned: {EID}, UUID: {UUID}", p.EntityId, p.PlayerUuid));

            packetStream
                .OfType<SpawnPlayerPacket>()
                .Where(p => p.PlayerUuid == interestUuid)
                .Subscribe(p =>
                {
                    Log.Fatal("Interest user spawned. EID: {EID}", p.EntityId);
                    interestEid = p.EntityId;
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
