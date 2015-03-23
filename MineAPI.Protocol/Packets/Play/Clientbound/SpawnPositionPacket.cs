using MineAPI.Common;

namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x05, PacketDirection.Clientbound)]
    public struct SpawnPositionPacket : IPacket
    {
        [PacketField(0, FieldType.Location)]
        public Position Location;
    }
}