namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x14, PacketDirection.Clientbound)]
    public struct EntityPacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int EntityId;
    }
}