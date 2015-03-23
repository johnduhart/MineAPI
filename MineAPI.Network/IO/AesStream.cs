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
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace MineAPI.Network.IO
{
    public class AesStream : Stream
    {
        private BufferedBlockCipher encryptCipher { get; set; }
        private BufferedBlockCipher decryptCipher { get; set; }

        public AesStream(Stream stream, byte[] key)
        {
            BaseStream = stream;
            encryptCipher = new BufferedBlockCipher(new CfbBlockCipher(new AesFastEngine(), 8));
            encryptCipher.Init(true, new ParametersWithIV(
                new KeyParameter(key), key, 0, 16));
            decryptCipher = new BufferedBlockCipher(new CfbBlockCipher(new AesFastEngine(), 8));
            decryptCipher.Init(false, new ParametersWithIV(
                new KeyParameter(key), key, 0, 16));
        }

        public Stream BaseStream { get; set; }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { return 0; } // hack for libnbt
            set { throw new NotSupportedException(); }
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override int ReadByte()
        {
            int value = BaseStream.ReadByte();
            if (value == -1) return value;
            return decryptCipher.ProcessByte((byte)value)[0];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int length = BaseStream.Read(buffer, offset, count);
            var decrypted = decryptCipher.ProcessBytes(buffer, offset, length);
            Array.Copy(decrypted, 0, buffer, offset, decrypted.Length);
            return length;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var encrypted = encryptCipher.ProcessBytes(buffer, offset, count);
            BaseStream.Write(encrypted, 0, encrypted.Length);
        }

        public override void Close()
        {
            BaseStream.Close();
        }
    }
}