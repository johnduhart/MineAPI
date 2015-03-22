using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MineAPI.Common.Logging;
using MineAPI.Protocol.IO;
using MineAPI.Protocol.Packets;

namespace MineAPI.Protocol
{
    public class PacketLocator
    {
        private static readonly ILog Log = LogProvider.For<PacketLocator>();

        private class PacketContainer
        {
            public PacketContainer()
            {
                ClientboudPackets = new Dictionary<byte, PacketInfo>();
                ServerboundPackets = new Dictionary<byte, PacketInfo>();
            }

            public IDictionary<byte, PacketInfo> ClientboudPackets { get; set; }
            public IDictionary<byte, PacketInfo> ServerboundPackets { get; set; }

            public IDictionary<byte, PacketInfo> this[PacketDirection direction]
            {
                get
                {
                    if (direction == PacketDirection.Serverbound)
                        return ServerboundPackets;

                    // Clientbound and both ways
                    return ClientboudPackets;
                }
            }
        }

        public class PacketInfo
        {
            public NetworkState State { get; set; }
            public PacketDirection Direction { get; set; }
            public byte Id { get; set; }
            public Type Type { get; set; }
            public List<PacketFieldAction> FieldActions { get; set; }

            public IPacket ReadPacket(IMinecraftStreamReader reader)
            {
                var packet = (IPacket) Activator.CreateInstance(Type);

                foreach (var packetFieldAction in FieldActions)
                {
                    packetFieldAction.ReaderAction(packet, reader);
                }

                return packet;
            }

            public void WritePacket(IPacket packet, IMinecraftStreamWriter writer)
            {
                foreach (var packetFieldAction in FieldActions)
                {
                    packetFieldAction.WriterAction(packet, writer);
                }
            }
        }

        private IDictionary<NetworkState, PacketContainer> _packet; 

        public void RegisterPackets()
        {
            Log.Debug("Building packet info...");

            _packet = new Dictionary<NetworkState, PacketContainer>();

            Type packetType = typeof (IPacket);
            var packets = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => packetType.IsAssignableFrom(t))
                .Select(t => new
                {
                    type = t,
                    attribute = t.GetCustomAttribute<PacketAttribute>()
                })
                .Where(x => x.attribute != null)
                .Select(x => BuildPacketInfo(x.type, x.attribute))
                .ToList();


            foreach (NetworkState state in Enum.GetValues(typeof(NetworkState)))
            {
                var container = new PacketContainer();

                foreach (var packet in packets.Where(p => p.State == state))
                {
                    container[packet.Direction].Add(packet.Id, packet);
                }

                _packet.Add(state, container);
            }

            Log.DebugFormat("Done. Built packet info for {0} packets", packets.Count);
        }

        private PacketInfo BuildPacketInfo(Type packetType, PacketAttribute attribute)
        {
            // Find all fields
            var fields = packetType.GetFields()
                .Select(f => new {f, a = f.GetCustomAttribute<PacketFieldAttribute>()})
                .Where(x => x.a != null)
                .OrderBy(x => x.a.Order);

            var actionList = new List<PacketFieldAction>();

            foreach (var field in fields)
            {
                actionList.Add(new PacketFieldAction
                {
                    ReaderAction = BuildReaderAction(field.f, field.a.Type),
                    WriterAction = BuildWriterAction(field.f, field.a.Type)
                });
            }

            return new PacketInfo
            {
                Type = packetType,
                Direction = attribute.Direction,
                Id = attribute.Id,
                State = attribute.State,
                FieldActions = actionList,
            };
        }

        private Action<IPacket, IMinecraftStreamReader> BuildReaderAction(FieldInfo fieldInfo, FieldType type)
        {
            switch (type)
            {
                case FieldType.Boolean:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadBool());
                case FieldType.UShort:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadUShort());
                case FieldType.Long:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadLong());
                case FieldType.String:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadString());
                case FieldType.VarInt:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadVarInt());
                case FieldType.ByteArray:
                    return (packet, reader) =>
                    {
                        var arrayLength = reader.ReadVarInt();
                        fieldInfo.SetValue(packet, reader.ReadByteArray(arrayLength));
                    };
                default:
                    Log.WarnFormat("No reader action for type {0}", type);
                    return (packet, reader) => { };
            }
        }

        private Action<IPacket, IMinecraftStreamWriter> BuildWriterAction(FieldInfo fieldInfo, FieldType type)
        {
            switch (type)
            {
                case FieldType.Boolean:
                    return (packet, writer) => writer.WriteBool((bool) fieldInfo.GetValue(packet));
                case FieldType.UShort:
                    return (packet, writer) => writer.WriteUShort((ushort) fieldInfo.GetValue(packet));
                case FieldType.Long:
                    return (packet, writer) => writer.WriteLong((long) fieldInfo.GetValue(packet));
                case FieldType.String:
                    return (packet, writer) => writer.WriteString((string) fieldInfo.GetValue(packet));
                case FieldType.VarInt:
                    return (packet, writer) => writer.WriteVarInt((int)fieldInfo.GetValue(packet));
                case FieldType.ByteArray:
                    return (packet, writer) =>
                    {
                        var array = (byte[]) fieldInfo.GetValue(packet);
                        writer.WriteVarInt(array.Length);
                        writer.WriteByteArray(array);
                    };
                default:
                    Log.WarnFormat("No writer action for type {0}", type);
                    return (packet, writer) => { };
            }
        }

        public class PacketFieldAction
        {
            public Action<IPacket, IMinecraftStreamReader> ReaderAction { get; set; }
            public Action<IPacket, IMinecraftStreamWriter> WriterAction { get; set; }
        }

        public PacketInfo FindPacketInfo(byte id, PacketDirection direction, NetworkState state)
        {
            var types = _packet[state][direction];
            PacketInfo packetInfo;
            if (!types.TryGetValue(id, out packetInfo))
                return null;

            return packetInfo;
        }

        public PacketInfo GetInfoForPacket(IPacket packet)
        {
            var attribute = packet.GetType().GetCustomAttribute<PacketAttribute>();

            return FindPacketInfo(attribute.Id, attribute.Direction, attribute.State);
        }
    }
}