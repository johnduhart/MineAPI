using MineAPI.Common;

namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x0E, PacketDirection.Clientbound)]
    public struct SpawnObjectPacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int EntityId;

        [PacketField(1, FieldType.Byte)]
        public sbyte Type;

        [PacketField(2, FieldType.Vector3FixedPoint)]
        public Vector3 Position;

        [PacketField(3, FieldType.Byte)]
        public sbyte Pitch;

        [PacketField(4, FieldType.Byte)]
        public sbyte Yaw;

        // TODO: Object data
    }
}