using MineAPI.Common;

namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x0F, PacketDirection.Clientbound)]
    public struct SpawnMobPacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int EntityId;

        [PacketField(1, FieldType.UByte)]
        public byte Type;

        [PacketField(2, FieldType.Vector3FixedPoint)]
        public Vector3 Position;

        [PacketField(3, FieldType.Byte)]
        public sbyte Yaw;

        [PacketField(4, FieldType.Byte)]
        public sbyte Pitch;

        [PacketField(5, FieldType.Byte)]
        public sbyte HeadPitch;

        [PacketField(6, FieldType.Short)]
        public short VelocityX;

        [PacketField(7, FieldType.Short)]
        public short VelocityY;

        [PacketField(8, FieldType.Short)]
        public short VelocityZ;

        // TODO: Metadata
    }
}