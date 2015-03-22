namespace MineAPI.Protocol.Packets.Login
{
    [Packet(0x00, PacketDirection.Serverbound, State = NetworkState.Login)]
    public struct LoginStartPacket : IPacket
    {
        [PacketField(0, FieldType.String)]
        public string Username;
    }

    [Packet(0x01, PacketDirection.Clientbound, State = NetworkState.Login)]
    public struct EncryptionRequestPacket : IPacket
    {
        [PacketField(0, FieldType.String)]
        public string ServerId;

        [PacketField(1, FieldType.ByteArray)]
        public byte[] PublicKey;

        [PacketField(2, FieldType.ByteArray)]
        public byte[] VerifyToken;
    }

    [Packet(0x02, PacketDirection.Clientbound, State = NetworkState.Login)]
    public struct LoginSuccessPacket : IPacket, IStateChangePacket
    {
        [PacketField(0, FieldType.String)]
        public string UUID;

        [PacketField(1, FieldType.String)]
        public string Username;

        public NetworkState NewState
        {
            get { return NetworkState.Play; }
        }
    }

    [Packet(0x03, PacketDirection.Clientbound, State = NetworkState.Login)]
    public struct SetCompressionPacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int Threshold;
    }

    [Packet(0x01, PacketDirection.Serverbound, State = NetworkState.Login)]
    public struct EncryptionResponsePacket : IPacket
    {
        [PacketField(0, FieldType.ByteArray)]
        public byte[] SharedSecret;

        [PacketField(1, FieldType.ByteArray)]
        public byte[] VerifyToken;
    }
}