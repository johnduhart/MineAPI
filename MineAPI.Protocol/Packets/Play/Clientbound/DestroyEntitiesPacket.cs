namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x13, PacketDirection.Clientbound)]
    public struct DestroyEntitiesPacket : IPacket
    {
        [PacketField(0, FieldType.VarIntArray)]
        public int[] EntityIds;
    }
}