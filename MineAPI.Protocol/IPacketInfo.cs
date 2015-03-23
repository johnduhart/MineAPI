using System;
using MineAPI.Protocol.IO;

namespace MineAPI.Protocol
{
    public interface IPacketInfo
    {
        NetworkState State { get; }

        PacketDirection Direction { get; }

        byte Id { get; }

        Type Type { get; }

        IPacket ReadPacketFromStream(IMinecraftStreamReader reader);

        void WritePacketToStream(IPacket packet, IMinecraftStreamWriter writer);
    }
}