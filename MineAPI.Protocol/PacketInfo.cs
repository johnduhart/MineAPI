using System;
using System.Collections.Generic;
using MineAPI.Protocol.IO;

namespace MineAPI.Protocol
{
    internal class PacketInfo : IPacketInfo
    {
        public NetworkState State { get; internal set; }
        public PacketDirection Direction { get; internal set; }
        public byte Id { get; internal set; }
        public Type Type { get; internal set; }
        internal List<PacketFieldAction> FieldActions { get; set; } 

        public IPacket ReadPacketFromStream(IMinecraftStreamReader reader)
        {
            var packet = (IPacket)Activator.CreateInstance(Type);

            foreach (var packetFieldAction in FieldActions)
            {
                packetFieldAction.ReaderAction(packet, reader);
            }

            return packet;
        }

        public void WritePacketToStream(IPacket packet, IMinecraftStreamWriter writer)
        {
            foreach (var packetFieldAction in FieldActions)
            {
                packetFieldAction.WriterAction(packet, writer);
            }
        }
    }
}