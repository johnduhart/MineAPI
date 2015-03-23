namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x04, PacketDirection.Clientbound)]
    public struct EntityEquipmentPacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int EntityId;

        [PacketField(1, FieldType.Short)]
        public short Slot;

        // TODO: Implement item
    }
}