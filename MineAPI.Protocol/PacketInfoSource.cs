using System;
using System.Collections.Generic;
using System.Linq;
using MineAPI.Common.Logging;

namespace MineAPI.Protocol
{
    public class PacketInfoSource : IPacketInfoSource
    {
        private static readonly ILog Log = LogProvider.For<PacketInfoSource>();

        private IDictionary<Type, PacketInfo> _packetsByType;
        private readonly IDictionary<NetworkState, StateContainer> _packetsByState = new Dictionary<NetworkState, StateContainer>();

        public PacketInfoSource()
        {
            BuildPacketInfo();
        }

        public IPacketInfo GetPacketInfo(byte packetId, PacketDirection direction, NetworkState state)
        {
            return _packetsByState[state].GetPacketInfo(direction, packetId);
        }

        public IPacketInfo GetPacketInfo(IPacket packet)
        {
            return GetPacketInfo(packet.GetType());
        }

        public IPacketInfo GetPacketInfo(Type packetType)
        {
            PacketInfo packetInfo;
            _packetsByType.TryGetValue(packetType, out packetInfo);

            return packetInfo;
        }

        private void BuildPacketInfo()
        {
            Log.Debug("Building packet info");

            IReadOnlyCollection<PacketInfo> packetInfo = new PacketInfoBuilder().BuildPacketInfo();

            _packetsByType = packetInfo.ToDictionary(p => p.Type);

            foreach (NetworkState state in Enum.GetValues(typeof(NetworkState)))
            {
                var container = new StateContainer();

                foreach (PacketInfo packet in packetInfo.Where(p => p.State == state))
                {
                    container.AddPacketInfo(packet);
                }

                _packetsByState.Add(state, container);
            }
        }

        class StateContainer
        {
            private readonly Dictionary<byte, PacketInfo> _clientboundPackets = new Dictionary<byte, PacketInfo>();
            private readonly Dictionary<byte, PacketInfo> _serverboundPackets = new Dictionary<byte, PacketInfo>(); 

            public void AddPacketInfo(PacketInfo packet)
            {
                if (packet.Direction == PacketDirection.Clientbound || packet.Direction == PacketDirection.Both)
                    _clientboundPackets.Add(packet.Id, packet);

                if (packet.Direction == PacketDirection.Serverbound || packet.Direction == PacketDirection.Both)
                    _serverboundPackets.Add(packet.Id, packet);
            }

            public PacketInfo GetPacketInfo(PacketDirection direction, byte packetId)
            {
                PacketInfo packet;
                if (direction == PacketDirection.Clientbound)
                {
                    _clientboundPackets.TryGetValue(packetId, out packet);
                }
                else
                {
                    _serverboundPackets.TryGetValue(packetId, out packet);
                }

                return packet;
            }
        }
    }
}