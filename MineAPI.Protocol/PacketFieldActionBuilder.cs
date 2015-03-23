using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MineAPI.Common.Logging;
using MineAPI.Protocol.IO;
using MineAPI.Protocol.Packets;

namespace MineAPI.Protocol
{
    internal class PacketFieldActionBuilder
    {
        private static readonly ILog Log = LogProvider.For<PacketInfoSource>();

        public void BuildActionsForPacket(PacketInfo packetInfo)
        {
            var actionList = new List<PacketFieldAction>();
            var fields = GetFieldsOnPacket(packetInfo.Type);

            foreach (var field in fields)
            {
                actionList.Add(new PacketFieldAction
                {
                    ReaderAction = BuildReaderAction(field.Item1, field.Item2.Type),
                    WriterAction = BuildWriterAction(field.Item1, field.Item2.Type),
                });
            }

            packetInfo.FieldActions = actionList;
        }

        private IEnumerable<Tuple<FieldInfo, PacketFieldAttribute>> GetFieldsOnPacket(Type type)
        {
            return type.GetFields()
                .Select(f => Tuple.Create<FieldInfo, PacketFieldAttribute>(f, CustomAttributeExtensions.GetCustomAttribute<PacketFieldAttribute>((MemberInfo) f)))
                .Where(t => t.Item2 != null)
                .OrderBy(t => t.Item2.Order);
        }

        private Action<IPacket, IMinecraftStreamReader> BuildReaderAction(FieldInfo fieldInfo, FieldType type)
        {
            switch (type)
            {
                case FieldType.Boolean:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadBool());
                case FieldType.Byte:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadByte());
                case FieldType.UByte:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadUByte());
                case FieldType.UShort:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadUShort());
                case FieldType.Int:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadInt());
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
                    return (packet, writer) => writer.WriteBool((bool)fieldInfo.GetValue(packet));
                case FieldType.Byte:
                    return (packet, writer) => writer.WriteByte((sbyte) fieldInfo.GetValue(packet));
                case FieldType.UByte:
                    return (packet, writer) => writer.WriteUByte((byte) fieldInfo.GetValue(packet));
                case FieldType.UShort:
                    return (packet, writer) => writer.WriteUShort((ushort)fieldInfo.GetValue(packet));
                case FieldType.Int:
                    return (packet, writer) => writer.WriteInt((int) fieldInfo.GetValue(packet));
                case FieldType.Long:
                    return (packet, writer) => writer.WriteLong((long)fieldInfo.GetValue(packet));
                case FieldType.String:
                    return (packet, writer) => writer.WriteString((string)fieldInfo.GetValue(packet));
                case FieldType.VarInt:
                    return (packet, writer) => writer.WriteVarInt((int)fieldInfo.GetValue(packet));
                case FieldType.ByteArray:
                    return (packet, writer) =>
                    {
                        var array = (byte[])fieldInfo.GetValue(packet);
                        writer.WriteVarInt(array.Length);
                        writer.WriteByteArray(array);
                    };
                default:
                    Log.WarnFormat("No writer action for type {0}", type);
                    return (packet, writer) => { };
            }
        }
    }
}