using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MineAPI.Protocol
{
    internal class PacketInfoBuilder
    {
        private readonly PacketFieldActionBuilder _fieldActionBuilder = new PacketFieldActionBuilder();

        public IReadOnlyCollection<PacketInfo> BuildPacketInfo()
        {
            var packetTypes = GetAllPacketTypes();

            return packetTypes.Select(BuildPacketInfoForType)
                .ToList();
        }

        private List<Type> GetAllPacketTypes()
        {
            Type packetInterface = typeof (IPacket);
            return GetType().Assembly.GetTypes()
                .Where(t => packetInterface.IsAssignableFrom(t))
                .Where(t => t.GetCustomAttribute<PacketAttribute>() != null)
                .ToList();
        }

        private PacketInfo BuildPacketInfoForType(Type packetType)
        {
            var packetAttribute = packetType.GetCustomAttribute<PacketAttribute>();

            var packetInfo = new PacketInfo
            {
                State = packetAttribute.State,
                Direction = packetAttribute.Direction,
                Id = packetAttribute.Id,
                Type = packetType,
            };

            _fieldActionBuilder.BuildActionsForPacket(packetInfo);

            return packetInfo;
        }
    }
}