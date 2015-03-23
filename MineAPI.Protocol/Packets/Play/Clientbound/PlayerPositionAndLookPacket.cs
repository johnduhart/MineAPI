using MineAPI.Common;

namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x08, PacketDirection.Clientbound)]
    public struct PlayerPositionAndLookPacket : IPacket
    {
        [PacketField(0, FieldType.Vector3)]
        public Vector3 Position;

        [PacketField(1, FieldType.Float)]
        public float Yaw;

        [PacketField(2, FieldType.Float)]
        public float Pitch;

        [PacketField(3, FieldType.Byte)]
        public PlayerPositionAndLookFlags Flags;
    }
}