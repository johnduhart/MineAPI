namespace MineAPI.Protocol.Packets.Play
{
    [Packet(0x02, PacketDirection.Clientbound)]
    public struct ClientChatMessagePacket : IPacket
    {
        [PacketField(0, FieldType.String)]
        public string MessageJson;

        [PacketField(1, FieldType.Byte)]
        public sbyte Position;
    }
}