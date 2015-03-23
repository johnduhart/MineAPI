namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x0A, PacketDirection.Clientbound)]
    public struct HeldItemChangePacket : IPacket
    {
        [PacketField(0, FieldType.Byte)]
        public sbyte Slot;
    }
}