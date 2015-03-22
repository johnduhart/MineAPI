namespace MineAPI.Network.IO
{
    public interface IMinecraftStreamWriter
    {
        void WriteBool(bool value);

        void WriteByte(sbyte value);
        void WriteUByte(byte value);

        void WriteShort(short value);
        void WriteUShort(ushort value);

        void WriteInt(int value);
        void WriteUInt(uint value);

        void WriteLong(long value);
        void WriteULong(ulong value);

        void WriteDouble(double value);

        void WriteFloat(float value);

        void WriteVarInt(int value);

        void WriteString(string value);
        
        void WriteByteArray(byte[] bytes);
    }
}