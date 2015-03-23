namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x16, PacketDirection.Clientbound)]
    public struct EntityLookPacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int EntityId;

        [PacketField(1, FieldType.Byte)]
        public sbyte Yaw;

        [PacketField(2, FieldType.Byte)]
        public sbyte Pitch;

        [PacketField(3, FieldType.Boolean)]
        public bool OnGround;
    }
}