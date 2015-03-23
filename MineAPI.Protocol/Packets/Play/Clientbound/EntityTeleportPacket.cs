using MineAPI.Common;

namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x18, PacketDirection.Clientbound)]
    public struct EntityTeleportPacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int EntityId;

        [PacketField(1, FieldType.Vector3FixedPoint)]
        public Vector3 Position;

        [PacketField(2, FieldType.Byte)]
        public sbyte Yaw;

        [PacketField(3, FieldType.Byte)]
        public sbyte Pitch;

        [PacketField(4, FieldType.Boolean)]
        public bool OnGround;
    }
}