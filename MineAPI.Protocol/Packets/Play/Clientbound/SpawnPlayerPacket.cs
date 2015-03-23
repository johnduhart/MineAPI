using MineAPI.Common;

namespace MineAPI.Protocol.Packets.Play.Clientbound
{
    [Packet(0x0C, PacketDirection.Clientbound)]
    public struct SpawnPlayerPacket : IPacket
    {
        [PacketField(0, FieldType.VarInt)]
        public int EntityId;

        [PacketField(1, FieldType.UUID)]
        public byte[] PlayerUuid;

        [PacketField(2, FieldType.Vector3FixedPoint)]
        public Vector3 Position;

        [PacketField(3, FieldType.Byte)]
        public sbyte Yaw;

        [PacketField(4, FieldType.Byte)]
        public sbyte Pitch;

        [PacketField(5, FieldType.Short)]
        public short CurrentItem;

        // TODO: Metadata
    }
}