namespace MineAPI.Protocol.IO
{
    public interface IMinecraftStreamReader
    {
        bool ReadBool();

        sbyte ReadByte();
        byte ReadUByte();

        byte[] ReadByteArray(int length);


        short ReadShort();
        ushort ReadUShort();

        int ReadInt();
        uint ReadUInt();

        long ReadLong();
        ulong ReadULong();

        float ReadFloat();

        double ReadDouble();

        int ReadVarInt();

        string ReadString();
        int ReadVarInt(out int length);
    }
}