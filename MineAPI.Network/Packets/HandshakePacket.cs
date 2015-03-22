using MineAPI.Network.IO;

namespace MineAPI.Network.Packets
{
    [Packet(0x00, PacketDirection.Serverbound)]
    public struct HandshakePacket : IPacket, IStateChangePacket
    {
        public int ProtocolVersion;
        public string ServerAddress;
        public ushort ServerPort;
        public NetworkState NextState;

        public NetworkState NewState
        {
            get { return NextState; }
        }

        public void ReadPacket(IMinecraftStreamReader reader)
        {
            throw new System.NotImplementedException();
        }

        public void WritePacket(IMinecraftStreamWriter writer)
        {
            writer.WriteVarInt(ProtocolVersion);
            writer.WriteString(ServerAddress);
            writer.WriteUShort(ServerPort);
            writer.WriteVarInt((int) NextState);
        }
    }
}