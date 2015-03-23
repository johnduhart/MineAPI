using System;

namespace MineAPI.Common
{
    public struct Vector3 : IEquatable<Vector3>
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Vector3(float value)
        {
            X = Y = Z = value;
        }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(double x, double y, double z)
        {
            X = (float)x;
            Y = (float)y;
            Z = (float)z;
        }

        public Vector3(Vector3 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public override string ToString()
        {
            return string.Format("X: {0}, Y: {1}, Z: {2}", X, Y, Z);
        }

        #region Eqaulity

        public bool Equals(Vector3 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Vector3 && Equals((Vector3) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode*397) ^ Y.GetHashCode();
                hashCode = (hashCode*397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !left.Equals(right);
        }

        #endregion

    }
}