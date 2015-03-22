using MineAPI.Network.IO;

namespace MineAPI.Network.Packets.Login
{
    [Packet(0x00, PacketDirection.Serverbound, State = NetworkState.Login)]
    public struct LoginStartPacket : IPacket
    {
        public string Username;

        public void ReadPacket(IMinecraftStreamReader reader)
        {
            Username = reader.ReadString();
        }

        public void WritePacket(IMinecraftStreamWriter writer)
        {
            writer.WriteString(Username);
        }
    }

    [Packet(0x01, PacketDirection.Clientbound, State = NetworkState.Login)]
    public struct EncryptionRequestPacket : IPacket
    {
        public string ServerId;
        public byte[] PublicKey;
        public byte[] VerifyToken;

        public void ReadPacket(IMinecraftStreamReader reader)
        {
            ServerId = reader.ReadString();
            int keyLength = reader.ReadVarInt();
            PublicKey = reader.ReadByteArray(keyLength);
            int tokenLength = reader.ReadVarInt();
            VerifyToken = reader.ReadByteArray(tokenLength);
        }

        public void WritePacket(IMinecraftStreamWriter writer)
        {
            writer.WriteString(ServerId);
            writer.WriteVarInt(PublicKey.Length);
            writer.WriteByteArray(PublicKey);
            writer.WriteVarInt(VerifyToken.Length);
            writer.WriteByteArray(VerifyToken);
        }
    }

    [Packet(0x02, PacketDirection.Clientbound, State = NetworkState.Login)]
    public struct LoginSuccessPacket : IPacket, IStateChangePacket
    {
        public string UUID;
        public string Username;

        public NetworkState NewState
        {
            get { return NetworkState.Play; }
        }

        public void ReadPacket(IMinecraftStreamReader reader)
        {
            UUID = reader.ReadString();
            Username = reader.ReadString();
        }

        public void WritePacket(IMinecraftStreamWriter writer)
        {
            writer.WriteString(UUID);
            writer.WriteString(Username);
        }
    }

    [Packet(0x03, PacketDirection.Clientbound, State = NetworkState.Login)]
    public struct SetCompressionPacket : IPacket
    {
        public int Threshold;

        public void ReadPacket(IMinecraftStreamReader reader)
        {
            Threshold = reader.ReadVarInt();
        }

        public void WritePacket(IMinecraftStreamWriter writer)
        {
            writer.WriteVarInt(Threshold);
        }
    }

    [Packet(0x01, PacketDirection.Serverbound, State = NetworkState.Login)]
    public struct EncryptionResponsePacket : IPacket
    {
        public byte[] SharedSecret;
        public byte[] VerifyToken;

        public void ReadPacket(IMinecraftStreamReader reader)
        {
            int sharedSecretLength = reader.ReadVarInt();
            SharedSecret = reader.ReadByteArray(sharedSecretLength);
            int tokenLength = reader.ReadVarInt();
            VerifyToken = reader.ReadByteArray(tokenLength);
        }

        public void WritePacket(IMinecraftStreamWriter writer)
        {
            writer.WriteVarInt(SharedSecret.Length);
            writer.WriteByteArray(SharedSecret);
            writer.WriteVarInt(VerifyToken.Length);
            writer.WriteByteArray(VerifyToken);
        }
    }
}