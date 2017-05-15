/* Copyright (c) 2017 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.IO;

namespace Gibbed.Rebirth.FileFormats
{
    internal class BitReader
    {
        private readonly byte[] _Buffer;
        private int _Position;
        private readonly int _Length;
        private int _RemainingBits;
        private uint _Value;

        public int Position
        {
            get { return this._Position; }
            set { this._Position = value; }
        }

        public int Length
        {
            get { return this._Length; }
        }

        public BitReader(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            this._Buffer = buffer;
            this._Position = 0;
            this._Length = buffer.Length;
        }

        public int ReadInt32(int bits)
        {
            return (int)this.ReadUInt32(bits);
        }

        public uint ReadUInt32(int bits)
        {
            if (bits <= 0 || bits > 32)
            {
                throw new ArgumentOutOfRangeException("bits", bits, "bits must be > 0 && <= 32");
            }

            while (this._RemainingBits < bits)
            {
                if (this._Position >= this._Length)
                {
                    throw new EndOfStreamException();
                }

                this._Value <<= 8;
                this._Value |= this._Buffer[this._Position++];
                this._RemainingBits += 8;
            }

            this._RemainingBits -= bits;
            uint mask = (1u << bits) - 1;
            var result = mask & (this._Value >> this._RemainingBits);
            return result;
        }
    }
}
