using System;
using MineAPI.Protocol.IO;

namespace MineAPI.Protocol
{
    internal class PacketFieldAction
    {
        public Action<IPacket, IMinecraftStreamReader> ReaderAction { get; set; }
        public Action<IPacket, IMinecraftStreamWriter> WriterAction { get; set; }
    }
}