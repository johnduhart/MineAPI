namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x01, PacketDirection.Clientbound)]
    public struct PlayGamePacket : IPacket
    {
        [PacketField(0, FieldType.Int)]
        public int EntityId;

        [PacketField(1, FieldType.UByte)]
        public byte Gamemode;

        [PacketField(2, FieldType.Byte)]
        public sbyte Dimension;

        [PacketField(3, FieldType.UByte)]
        public byte Difficulty;

        [PacketField(4, FieldType.UByte)]
        public byte MaxPlayers;

        [PacketField(5, FieldType.String)]
        public string LevelType;

        [PacketField(6, FieldType.Boolean)]
        public bool ReducedDebugInfo;
    }
}