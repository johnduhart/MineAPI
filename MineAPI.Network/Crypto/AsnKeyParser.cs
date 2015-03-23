// Copyright (c) 2011-2013 Drew DeVault
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Math;

namespace MineAPI.Network.Crypto
{
    public class AsnKeyParser
    {
        private AsnParser parser;

        public AsnKeyParser(String pathname)
        {
            using (BinaryReader reader = new BinaryReader(
                new FileStream(pathname, FileMode.Open, FileAccess.Read)))
            {
                FileInfo info = new FileInfo(pathname);

                parser = new AsnParser(reader.ReadBytes((int)info.Length));
            }
        }

        public AsnKeyParser(byte[] data)
        {
            parser = new AsnParser(data);
        }

        internal static byte[] TrimLeadingZero(byte[] values)
        {
            byte[] r = null;
            if ((0x00 == values[0]) && (values.Length > 1))
            {
                r = new byte[values.Length - 1];
                Array.Copy(values, 1, r, 0, values.Length - 1);
            }
            else
            {
                r = new byte[values.Length];
                Array.Copy(values, r, values.Length);
            }

            return r;
        }

        internal static bool EqualOid(byte[] first, byte[] second)
        {
            if (first.Length != second.Length)
            { return false; }

            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                { return false; }
            }

            return true;
        }

        public RSAParameters ParseRSAPublicKey()
        {
            RSAParameters parameters = new RSAParameters();

            // Current value
            byte[] value = null;

            // Sanity Check
            int length = 0;

            // Checkpoint
            int position = parser.CurrentPosition();

            // Ignore Sequence - PublicKeyInfo
            length = parser.NextSequence();
            if (length != parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect Sequence Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture),
                  parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Checkpoint
            position = parser.CurrentPosition();

            // Ignore Sequence - AlgorithmIdentifier
            length = parser.NextSequence();
            if (length > parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect AlgorithmIdentifier Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture),
                  parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Checkpoint
            position = parser.CurrentPosition();
            // Grab the OID
            value = parser.NextOID();
            byte[] oid = { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 };
            if (!EqualOid(value, oid))
            { throw new BerDecodeException("Expected OID 1.2.840.113549.1.1.1", position); }

            // Optional Parameters
            if (parser.IsNextNull())
            {
                parser.NextNull();
                // Also OK: value = parser.Next();
            }
            else
            {
                // Gracefully skip the optional data
                value = parser.Next();
            }

            // Checkpoint
            position = parser.CurrentPosition();

            // Ignore BitString - PublicKey
            length = parser.NextBitString();
            if (length > parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect PublicKey Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture),
                  (parser.RemainingBytes()).ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Checkpoint
            position = parser.CurrentPosition();

            // Ignore Sequence - RSAPublicKey
            length = parser.NextSequence();
            if (length < parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect RSAPublicKey Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture),
                  parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            parameters.Modulus = TrimLeadingZero(parser.NextInteger());
            parameters.Exponent = TrimLeadingZero(parser.NextInteger());

            Debug.Assert(0 == parser.RemainingBytes());

            return parameters;
        }

        internal RSAParameters ParseRSAPrivateKey()
        {
            RSAParameters parameters = new RSAParameters();

            // Current value
            byte[] value = null;

            // Checkpoint
            int position = parser.CurrentPosition();

            // Sanity Check
            int length = 0;

            // Ignore Sequence - PrivateKeyInfo
            length = parser.NextSequence();
            if (length != parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect Sequence Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture), parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Checkpoint
            position = parser.CurrentPosition();
            // Version
            value = parser.NextInteger();
            if (0x00 != value[0])
            {
                StringBuilder sb = new StringBuilder("Incorrect PrivateKeyInfo Version. ");
                BigInteger v = new BigInteger(value);
                sb.AppendFormat("Expected: 0, Specified: {0}", v.ToString(10));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Checkpoint
            position = parser.CurrentPosition();

            // Ignore Sequence - AlgorithmIdentifier
            length = parser.NextSequence();
            if (length > parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect AlgorithmIdentifier Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture),
                  parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Checkpoint
            position = parser.CurrentPosition();

            // Grab the OID
            value = parser.NextOID();
            byte[] oid = { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 };
            if (!EqualOid(value, oid))
            { throw new BerDecodeException("Expected OID 1.2.840.113549.1.1.1", position); }

            // Optional Parameters
            if (parser.IsNextNull())
            {
                parser.NextNull();
                // Also OK: value = parser.Next();
            }
            else
            {
                // Gracefully skip the optional data
                value = parser.Next();
            }

            // Checkpoint
            position = parser.CurrentPosition();

            // Ignore OctetString - PrivateKey
            length = parser.NextOctetString();
            if (length > parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect PrivateKey Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture),
                  parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Checkpoint
            position = parser.CurrentPosition();

            // Ignore Sequence - RSAPrivateKey
            length = parser.NextSequence();
            if (length < parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect RSAPrivateKey Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture),
                  parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Checkpoint
            position = parser.CurrentPosition();
            // Version
            value = parser.NextInteger();
            if (0x00 != value[0])
            {
                StringBuilder sb = new StringBuilder("Incorrect RSAPrivateKey Version. ");
                BigInteger v = new BigInteger(value);
                sb.AppendFormat("Expected: 0, Specified: {0}", v.ToString(10));
                throw new BerDecodeException(sb.ToString(), position);
            }

            parameters.Modulus = TrimLeadingZero(parser.NextInteger());
            parameters.Exponent = TrimLeadingZero(parser.NextInteger());
            parameters.D = TrimLeadingZero(parser.NextInteger());
            parameters.P = TrimLeadingZero(parser.NextInteger());
            parameters.Q = TrimLeadingZero(parser.NextInteger());
            parameters.DP = TrimLeadingZero(parser.NextInteger());
            parameters.DQ = TrimLeadingZero(parser.NextInteger());
            parameters.InverseQ = TrimLeadingZero(parser.NextInteger());

            Debug.Assert(0 == parser.RemainingBytes());

            return parameters;
        }

        internal DSAParameters ParseDSAPublicKey()
        {
            DSAParameters parameters = new DSAParameters();

            // Current value
            byte[] value = null;

            // Current Position
            int position = parser.CurrentPosition();
            // Sanity Checks
            int length = 0;

            // Ignore Sequence - PublicKeyInfo
            length = parser.NextSequence();
            if (length != parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect Sequence Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture), parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Checkpoint
            position = parser.CurrentPosition();

            // Ignore Sequence - AlgorithmIdentifier
            length = parser.NextSequence();
            if (length > parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect AlgorithmIdentifier Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture), parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Checkpoint
            position = parser.CurrentPosition();

            // Grab the OID
            value = parser.NextOID();
            byte[] oid = { 0x2a, 0x86, 0x48, 0xce, 0x38, 0x04, 0x01 };
            if (!EqualOid(value, oid))
            { throw new BerDecodeException("Expected OID 1.2.840.10040.4.1", position); }


            // Checkpoint
            position = parser.CurrentPosition();

            // Ignore Sequence - DSS-Params
            length = parser.NextSequence();
            if (length > parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect DSS-Params Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture), parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Next three are curve parameters
            parameters.P = TrimLeadingZero(parser.NextInteger());
            parameters.Q = TrimLeadingZero(parser.NextInteger());
            parameters.G = TrimLeadingZero(parser.NextInteger());

            // Ignore BitString - PrivateKey
            parser.NextBitString();

            // Public Key
            parameters.Y = TrimLeadingZero(parser.NextInteger());

            Debug.Assert(0 == parser.RemainingBytes());

            return parameters;
        }

        internal DSAParameters ParseDSAPrivateKey()
        {
            DSAParameters parameters = new DSAParameters();

            // Current value
            byte[] value = null;

            // Current Position
            int position = parser.CurrentPosition();
            // Sanity Checks
            int length = 0;

            // Ignore Sequence - PrivateKeyInfo
            length = parser.NextSequence();
            if (length != parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect Sequence Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture), parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Checkpoint
            position = parser.CurrentPosition();
            // Version
            value = parser.NextInteger();
            if (0x00 != value[0])
            {
                throw new BerDecodeException("Incorrect PrivateKeyInfo Version", position);
            }

            // Checkpoint
            position = parser.CurrentPosition();

            // Ignore Sequence - AlgorithmIdentifier
            length = parser.NextSequence();
            if (length > parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect AlgorithmIdentifier Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture), parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Checkpoint
            position = parser.CurrentPosition();
            // Grab the OID
            value = parser.NextOID();
            byte[] oid = { 0x2a, 0x86, 0x48, 0xce, 0x38, 0x04, 0x01 };
            if (!EqualOid(value, oid))
            { throw new BerDecodeException("Expected OID 1.2.840.10040.4.1", position); }

            // Checkpoint
            position = parser.CurrentPosition();

            // Ignore Sequence - DSS-Params
            length = parser.NextSequence();
            if (length > parser.RemainingBytes())
            {
                StringBuilder sb = new StringBuilder("Incorrect DSS-Params Size. ");
                sb.AppendFormat("Specified: {0}, Remaining: {1}",
                  length.ToString(CultureInfo.InvariantCulture), parser.RemainingBytes().ToString(CultureInfo.InvariantCulture));
                throw new BerDecodeException(sb.ToString(), position);
            }

            // Next three are curve parameters
            parameters.P = TrimLeadingZero(parser.NextInteger());
            parameters.Q = TrimLeadingZero(parser.NextInteger());
            parameters.G = TrimLeadingZero(parser.NextInteger());

            // Ignore OctetString - PrivateKey
            parser.NextOctetString();

            // Private Key
            parameters.X = TrimLeadingZero(parser.NextInteger());

            Debug.Assert(0 == parser.RemainingBytes());

            return parameters;
        }
    }
}