namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x0D, PacketDirection.Clientbound)]
    public struct CollectItemPacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int CollectedEntityId;

        [PacketField(1, FieldType.VarInt)]
        public int CollectorEntityId;
    }
}