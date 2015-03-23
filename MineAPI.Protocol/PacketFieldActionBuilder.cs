using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MineAPI.Common;
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
                case FieldType.Float:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadFloat());
                case FieldType.String:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadString());
                case FieldType.VarInt:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadVarInt());
                case FieldType.Location:
                    return (packet, reader) => fieldInfo.SetValue(packet, Position.FromLong(reader.ReadLong()));
                case FieldType.Vector3:
                    return
                        (packet, reader) =>
                            fieldInfo.SetValue(packet,
                                new Vector3(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble()));
                case FieldType.Vector3FixedPoint:
                    return
                        (packet, reader) =>
                            fieldInfo.SetValue(packet,
                                new Vector3(reader.ReadInt() / 32.0f, reader.ReadInt() / 32.0f, reader.ReadInt() / 32.0f));
                case FieldType.Vector3FixedPointByte:
                    return
                        (packet, reader) =>
                            fieldInfo.SetValue(packet,
                                new Vector3(reader.ReadByte() / 32.0f, reader.ReadByte() / 32.0f, reader.ReadByte() / 32.0f));
                case FieldType.UUID:
                    return (packet, reader) => fieldInfo.SetValue(packet, reader.ReadByteArray(16));
                case FieldType.ByteArray:
                    return (packet, reader) =>
                    {
                        var arrayLength = reader.ReadVarInt();
                        fieldInfo.SetValue(packet, reader.ReadByteArray(arrayLength));
                    };
                case FieldType.VarIntArray:
                    return (packet, reader) =>
                    {
                        var arrayLength = reader.ReadVarInt();
                        int[] array = new int[arrayLength];
                        
                        for (int i = 0; i < arrayLength; i++)
                        {
                            array[i] = reader.ReadVarInt();
                        }

                        fieldInfo.SetValue(packet, array);
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
                case FieldType.Float:
                    return (packet, writer) => writer.WriteFloat((float)fieldInfo.GetValue(packet));
                case FieldType.String:
                    return (packet, writer) => writer.WriteString((string)fieldInfo.GetValue(packet));
                case FieldType.VarInt:
                    return (packet, writer) => writer.WriteVarInt((int)fieldInfo.GetValue(packet));
                case FieldType.Location:
                    return (packet, writer) => writer.WriteLong(((Position) fieldInfo.GetValue(packet)).ToLong());
                case FieldType.Vector3:
                    return (packet, writer) =>
                    {
                        var vector = (Vector3)fieldInfo.GetValue(packet);
                        writer.WriteDouble(vector.X);
                        writer.WriteDouble(vector.Y);
                        writer.WriteDouble(vector.Z);
                    };
                case FieldType.Vector3FixedPoint:
                    return (packet, writer) =>
                    {
                        var vector = (Vector3)fieldInfo.GetValue(packet);
                        writer.WriteInt((int)(vector.X * 32));
                        writer.WriteInt((int)(vector.Y * 32));
                        writer.WriteInt((int)(vector.Z * 32));
                    };
                case FieldType.Vector3FixedPointByte:
                    return (packet, writer) =>
                    {
                        var vector = (Vector3)fieldInfo.GetValue(packet);
                        writer.WriteByte((sbyte) (vector.X * 32));
                        writer.WriteByte((sbyte) (vector.Y * 32));
                        writer.WriteByte((sbyte) (vector.Z * 32));
                    };
                case FieldType.ByteArray:
                    return (packet, writer) =>
                    {
                        var array = (byte[])fieldInfo.GetValue(packet);
                        writer.WriteVarInt(array.Length);
                        writer.WriteByteArray(array);
                    };
                case FieldType.VarIntArray:
                    return (packet, writer) =>
                    {
                        var array = (int[])fieldInfo.GetValue(packet);
                        writer.WriteVarInt(array.Length);

                        foreach (int i in array)
                        {
                            writer.WriteVarInt(i);
                        }
                    };
                default:
                    Log.WarnFormat("No writer action for type {0}", type);
                    return (packet, writer) => { };
            }
        }
    }
}