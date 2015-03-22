using MineAPI.Network.IO;

namespace MineAPI.Network
{
    public interface IPacket
    {
        void ReadPacket(IMinecraftStreamReader reader);
        void WritePacket(IMinecraftStreamWriter writer);
    }
}