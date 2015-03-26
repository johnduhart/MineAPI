using MineAPI.Protocol.IO;

namespace MineAPI.Protocol
{
    public interface IPacket
    {
    }

    public interface IManualPacket : IPacket
    {
        void ReadPacket(IMinecraftStreamReader reader);
        void WritePacket(IMinecraftStreamWriter writer);
    }
}