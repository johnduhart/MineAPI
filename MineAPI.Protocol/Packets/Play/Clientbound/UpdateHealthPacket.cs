namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x06, PacketDirection.Clientbound)]
    public struct UpdateHealthPacket : IPacket
    {
        [PacketField(0, FieldType.Float)]
        public float Health;

        [PacketField(1, FieldType.VarInt)]
        public int Food;

        [PacketField(2, FieldType.Float)]
        public float FoodSaturation;
    }
}