using System;
using System.IO;
using System.Text;

namespace MineAPI.Protocol.IO
{
    public class MinecraftStreamWriter : IMinecraftStreamWriter
    {
        private readonly Stream _baseStream;

        public MinecraftStreamWriter(Stream baseStream)
        {
            _baseStream = baseStream;
        }

        public void WriteBool(bool value)
        {
            WriteUByte(value ? (byte)1 : (byte)0);
        }

        public void WriteByte(sbyte value)
        {
            _baseStream.WriteByte((byte) value);
        }

        public void WriteUByte(byte value)
        {
            _baseStream.WriteByte(value);            
        }

        public void WriteShort(short value)
        {
            WriteUShort((ushort) value);
        }

        public void WriteUShort(ushort value)
        {
            _baseStream.Write(new[]
            {
                (byte) ((value & 0xFF00) >> 8),
                (byte) (value & 0xFF),
            }, 0, 2);
        }

        public void WriteInt(int value)
        {
            WriteUInt((uint) value);
        }

        public void WriteUInt(uint value)
        {
            _baseStream.Write(new[]
            {
                (byte)((value & 0xFF000000) >> 24),
                (byte)((value & 0xFF0000) >> 16),
                (byte)((value & 0xFF00) >> 8),
                (byte)(value & 0xFF)
            }, 0, 4);
        }

        public void WriteLong(long value)
        {
            WriteULong((ulong) value);
        }

        public void WriteULong(ulong value)
        {
            _baseStream.Write(new[]
            {
                (byte)((value & 0xFF00000000000000) >> 56),
                (byte)((value & 0xFF000000000000) >> 48),
                (byte)((value & 0xFF0000000000) >> 40),
                (byte)((value & 0xFF00000000) >> 32),
                (byte)((value & 0xFF000000) >> 24),
                (byte)((value & 0xFF0000) >> 16),
                (byte)((value & 0xFF00) >> 8),
                (byte)(value & 0xFF)
            }, 0, 8);
        }

        public void WriteDouble(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);

            _baseStream.Write(bytes, 0, bytes.Length);
        }

        public void WriteFloat(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);

            _baseStream.Write(bytes, 0, bytes.Length);
        }

        public void WriteVarInt(int value)
        {
            var unsignedValue = (uint) value;
            while (true)
            {
                if ((value & 0xFFFFFF80u) == 0)
                {
                    WriteUByte((byte)value);
                    break;
                }
                WriteUByte((byte)(value & 0x7F | 0x80));
                value >>= 7;
            }
        }

        public void WriteString(string value)
        {
            var encoding = Encoding.UTF8;
            WriteVarInt(encoding.GetByteCount(value));

            if (value.Length > 0)
            {
                var bytes = encoding.GetBytes(value);
                _baseStream.Write(bytes, 0, bytes.Length);
            }
        }

        public void WriteByteArray(byte[] bytes)
        {
            _baseStream.Write(bytes, 0, bytes.Length);
        }
    }
}