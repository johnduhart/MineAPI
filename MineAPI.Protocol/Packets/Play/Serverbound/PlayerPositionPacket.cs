using MineAPI.Common;

namespace MineAPI.Protocol.Packets.Play.Serverbound
{
    [Packet(0x04, PacketDirection.Serverbound)]
    public struct PlayerPositionPacket : IPacket
    {
        [PacketField(0, FieldType.Vector3)]
        public Vector3 Position;

        [PacketField(1, FieldType.Boolean)]
        public bool OnGround;
    }
}