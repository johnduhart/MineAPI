namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x0B, PacketDirection.Clientbound)]
    public struct AnimationPacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int EntityId;

        [PacketField(1, FieldType.UByte)]
        public byte Animation;
    }
}