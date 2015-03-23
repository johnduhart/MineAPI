namespace MineAPI.Protocol.Packets.Play.Serverbound
{
    [Packet(0x16, PacketDirection.Serverbound)]
    public struct ClientStatusPacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public ClientStatusAction Action;
    }
}