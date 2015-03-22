using MineAPI.Network.IO;

namespace MineAPI.Network.Packets.Status
{
    [Packet(0x00, PacketDirection.Serverbound, State = NetworkState.Status)]
    public struct StatusRequestPacket : IPacket
    {
        public void ReadPacket(IMinecraftStreamReader reader)
        {
        }

        public void WritePacket(IMinecraftStreamWriter writer)
        {
        }
    }

    [Packet(0x00, PacketDirection.Clientbound, State = NetworkState.Status)]
    public struct StatusResponsePacket : IPacket
    {
        public string Response;

        public void ReadPacket(IMinecraftStreamReader reader)
        {
            // TODO: Parse JSON response
            Response = reader.ReadString();
        }

        public void WritePacket(IMinecraftStreamWriter writer)
        {
            writer.WriteString(Response);
        }
    }

    [Packet(0x01, PacketDirection.Both, State = NetworkState.Status)]
    public struct StatusTimePacket : IPacket
    {
        public long Time;

        public void ReadPacket(IMinecraftStreamReader reader)
        {
            Time = reader.ReadLong();
        }

        public void WritePacket(IMinecraftStreamWriter writer)
        {
            writer.WriteLong(Time);
        }
    }
}