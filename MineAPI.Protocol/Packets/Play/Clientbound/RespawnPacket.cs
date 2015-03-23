namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x07, PacketDirection.Clientbound)]
    public struct RespawnPacket : IPacket
    {
        [PacketField(0, FieldType.Int)]
        public int Dimension;

        [PacketField(1, FieldType.UByte)]
        public byte Difficulty;

        [PacketField(2, FieldType.UByte)]
        public byte Gamemode;

        [PacketField(3, FieldType.String)]
        public string LevelType;
    }
}