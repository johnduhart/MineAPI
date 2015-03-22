namespace MineAPI.Network.Packets.Status
{
    [Packet(0x00, PacketDirection.Serverbound, State = NetworkState.Status)]
    public struct StatusRequestPacket : IPacket
    {
    }

    [Packet(0x00, PacketDirection.Clientbound, State = NetworkState.Status)]
    public struct StatusResponsePacket : IPacket
    {
        [PacketField(0, FieldType.String)]
        public string Response;
    }

    [Packet(0x01, PacketDirection.Both, State = NetworkState.Status)]
    public struct StatusTimePacket : IPacket
    {
        [PacketField(0, FieldType.Long)]
        public long Time;
    }
}