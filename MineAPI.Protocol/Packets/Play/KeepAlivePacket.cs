namespace MineAPI.Protocol.Packets.Play
{
    [Packet(0x00, PacketDirection.Both)]
    public struct KeepAlivePacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int Id;
    }
}