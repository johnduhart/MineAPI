using System;
using JetBrains.Annotations;

namespace MineAPI.Protocol
{
    public interface IPacketInfoSource
    {
        IPacketInfo GetPacketInfo(byte packetId, PacketDirection direction, NetworkState state);

        [CanBeNull]
        IPacketInfo GetPacketInfo(IPacket packet);

        [CanBeNull]
        IPacketInfo GetPacketInfo(Type packetType);
    }
}