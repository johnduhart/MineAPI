using System;

namespace MineAPI.Common
{
    /// <summary>
    /// Represents the location of an object in 3D space (int).
    /// </summary>
    public struct Position : IEquatable<Position>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public Position(int value)
        {
            X = Y = Z = value;
        }

        public Position(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Position(Position v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public static Position FromLong(long value)
        {
            return new Position
            (
                (int)(value >> 38),
                (int)(value >> 26) & 0xFFF,
                (int)value << 38 >> 38
            );
        }

        #region Equality

        public long ToLong()
        {
            return ((X & 0x3FFFFFF) << 38) | ((Y & 0xFFF) << 26) | (Z & 0x3FFFFFF);
        }

        public bool Equals(Position other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Position && Equals((Position) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X;
                hashCode = (hashCode*397) ^ Y;
                hashCode = (hashCode*397) ^ Z;
                return hashCode;
            }
        }

        public static bool operator ==(Position left, Position right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !left.Equals(right);
        }

        #endregion

    }
}