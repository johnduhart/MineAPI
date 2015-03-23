using MineAPI.Common;

namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x17, PacketDirection.Clientbound)]
    public struct EntityLookAndRelativeMovePacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int EntityId;

        [PacketField(1, FieldType.Vector3FixedPointByte)]
        public Vector3 DeltaPosition;

        [PacketField(2, FieldType.Byte)]
        public sbyte Yaw;

        [PacketField(3, FieldType.Byte)]
        public sbyte Pitch;

        [PacketField(4, FieldType.Boolean)]
        public bool OnGround;
    }
}