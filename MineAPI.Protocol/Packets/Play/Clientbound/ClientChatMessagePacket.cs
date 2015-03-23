namespace MineAPI.Protocol.Packets.Play.Clientbound
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