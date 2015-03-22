using System;
using JetBrains.Annotations;

namespace MineAPI.Network
{
    [AttributeUsage(AttributeTargets.Struct)]
    [BaseTypeRequired(typeof(IPacket))]
    [MeansImplicitUse]
    public class PacketAttribute : Attribute
    {
        public byte Id { get; set; }
        public PacketDirection Direction { get; set; }
        public NetworkState State { get; set; }

        public PacketAttribute(byte id, PacketDirection direction)
        {
            Id = id;
            Direction = direction;
        }
    }
}