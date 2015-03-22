using System;
using JetBrains.Annotations;

namespace MineAPI.Network.Packets
{
    [AttributeUsage(AttributeTargets.Field)]
    [MeansImplicitUse]
    public class PacketFieldAttribute : Attribute
    {
        public int Order { get; private set; }
        public FieldType Type { get; private set; }

        public PacketFieldAttribute(int order, FieldType type)
        {
            Order = order;
            Type = type;
        }
    }
}