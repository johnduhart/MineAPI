namespace MineAPI.Protocol.Packets
{
    [Packet(0x00, PacketDirection.Serverbound)]
    public struct HandshakePacket : IPacket, IStateChangePacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int ProtocolVersion;

        [PacketField(1, FieldType.String)]
        public string ServerAddress;

        [PacketField(2, FieldType.UShort)]
        public ushort ServerPort;

        [PacketField(3, FieldType.VarInt)]
        public NetworkState NextState;

        public NetworkState NewState
        {
            get { return NextState; }
        }
    }
}