using System;
using System.Linq;

namespace MineAPI.Common
{
    public struct PlayerUuid : IEquatable<PlayerUuid>
    {
        private readonly byte[] _bytes;

        public PlayerUuid(byte[] bytes)
        {
            _bytes = bytes;
        }

        public byte[] ToByteArray()
        {
            return _bytes;
        }

        public override string ToString()
        {
            return new Guid(_bytes).ToString();
        }

        public bool Equals(PlayerUuid other)
        {
            return _bytes.SequenceEqual(other._bytes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PlayerUuid && Equals((PlayerUuid) obj);
        }

        public override int GetHashCode()
        {
            return (_bytes != null ? _bytes.GetHashCode() : 0);
        }

        public static bool operator ==(PlayerUuid left, PlayerUuid right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerUuid left, PlayerUuid right)
        {
            return !left.Equals(right);
        }
    }
}