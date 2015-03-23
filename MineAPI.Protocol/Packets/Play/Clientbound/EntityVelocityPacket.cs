namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x12, PacketDirection.Clientbound)]
    public struct EntityVelocityPacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int EntityId;

        [PacketField(1, FieldType.Short)]
        public short VelocityX;

        [PacketField(2, FieldType.Short)]
        public short VelocityY;

        [PacketField(3, FieldType.Short)]
        public short VelocityZ;
    }
}