using MineAPI.Common;

namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x15, PacketDirection.Clientbound)]
    public struct EntityRelativeMovePacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int EntityId;

        [PacketField(1, FieldType.Vector3FixedPointByte)]
        public Vector3 DeltaPosition;

        [PacketField(2, FieldType.Boolean)]
        public bool OnGround;
    }
}