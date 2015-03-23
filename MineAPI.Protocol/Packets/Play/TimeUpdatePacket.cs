namespace MineAPI.Protocol.Packets.Play
{
    [Packet(0x03, PacketDirection.Clientbound)]
    public struct TimeUpdatePacket : IPacket
    {
        [PacketField(0, FieldType.Long)]
        public long WorldAge;

        [PacketField(1, FieldType.Long)]
        public long TimeOfDay;
    }
}