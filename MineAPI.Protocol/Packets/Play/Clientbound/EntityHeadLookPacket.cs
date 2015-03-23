namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x19, PacketDirection.Clientbound)]
    public struct EntityHeadLookPacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int EntityId;

        [PacketField(1, FieldType.Byte)]
        public sbyte HeadYaw;
    }
}