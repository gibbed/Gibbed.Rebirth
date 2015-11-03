/* Copyright (c) 2015 Rick (rick 'at' gibbed 'dot' us)
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.IO;

namespace Gibbed.Rebirth.FileFormats
{
    public class ArchiveFile
    {
        private static readonly byte[] _Bogocrypt2Table;

        static ArchiveFile()
        {
            _Bogocrypt2Table = GetBogocrypt2Table();
        }

        private readonly byte[] _Signature = { 0x41, 0x52, 0x43, 0x48, 0x30, 0x30, 0x30, };

        private Endian _Endian;
        private ArchiveCompressionMode _CompressionMode;
        private readonly List<Entry> _Entries;

        public ArchiveFile()
        {
            this._Entries = new List<Entry>();
        }

        public Endian Endian
        {
            get { return this._Endian; }
            set { this._Endian = value; }
        }

        public ArchiveCompressionMode CompressionMode
        {
            get { return this._CompressionMode; }
            set { this._CompressionMode = value; }
        }

        public List<Entry> Entries
        {
            get { return this._Entries; }
        }

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            const Endian endian = Endian.Little;

            var basePosition = input.Position;

            var magic = input.ReadBytes(7);
            if (magic.SequenceEqual(_Signature) == false)
            {
                throw new FormatException();
            }

            var compressionMode = (ArchiveCompressionMode)input.ReadValueU8();
            var indexTableOffset = input.ReadValueU32(endian);
            var indexTableCount = input.ReadValueU16(endian);

            this._Endian = endian;
            this._CompressionMode = compressionMode;
            this._Entries.Clear();

            if (indexTableCount > 0)
            {
                var entries = new Entry[indexTableCount];
                input.Seek(basePosition + indexTableOffset, SeekOrigin.Begin);
                for (int i = 0; i < indexTableCount; i++)
                {
                    Entry entry;
                    entry.NameHashA = input.ReadValueU32(endian);
                    entry.NameHashB = input.ReadValueU32(endian);
                    entry.Offset = input.ReadValueU32(endian);
                    entry.Length = input.ReadValueU32(endian);
                    entry.Checksum = input.ReadValueU32(endian);
                    entries[i] = entry;
                }
                this._Entries.AddRange(entries);
            }
        }

        public struct Entry
        {
            public uint NameHashA;
            public uint NameHashB;
            public uint Offset;
            public uint Length;
            public uint Checksum;

            public ulong CombinedNameHash
            {
                get
                {
                    // ReSharper disable RedundantCast
                    return ((ulong)this.NameHashA) | ((ulong)this.NameHashB) << 32;
                    // ReSharper restore RedundantCast
                }
            }

            public uint BogocryptKey
            {
                get { return (this.NameHashB ^ 0xF9524287u) | 1u; }
            }

            // ReSharper disable InconsistentNaming
            public ISAAC GetISAAC()
                // ReSharper restore InconsistentNaming
            {
                var seed = (ulong)this.NameHashB;
                seed = (seed << 32) | (uint)(seed ^ ((seed ^ (seed << 15)) << 8) ^ (seed >> 9));

                var data = new int[256];
                for (int i = 0; i < data.Length; i++)
                {
                    var part = (uint)((seed >> 27) ^ (seed >> 45));
                    var shift = (int)(seed >> 59);

                    data[i] = (int)((part >> shift) | (part << ((-shift) & 31)));

                    seed *= 6364136223846793005L;
                    seed += 127;
                }

                return new ISAAC(data);
            }
        }

        public static ulong ComputeNameHash(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            path = path.ToLowerInvariant();
            ulong hash = ComputeNameHashA(path);
            hash |= ((ulong)ComputeNameHashB(path)) << 32;
            return hash;
        }

        private static uint ComputeNameHashA(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            uint hash = 5381;
            foreach (var c in path)
            {
                hash *= 33;
                hash += (byte)c;
            }
            return hash;
        }

        private static uint ComputeNameHashB(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            uint hash = 0x5BB2220E;
            foreach (var c in path)
            {
                hash ^= c;
                hash *= 0x1000193;
            }
            return hash;
        }

        public static uint ComputeChecksum(byte[] bytes, int offset, int count)
        {
            return ComputeChecksum(bytes, offset, count, 0xABABEB98u);
        }

        public static uint ComputeChecksum(byte[] bytes, int offset, int count, uint seed)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            // due to OOB, 1-3 bytes of earlier blocks can 'bleed' into future blocks
            // thus, this code is more inefficient than it could be
            uint hash = seed;
            var block = new byte[512];
            while (count > 0)
            {
                var blockLength = Math.Min(512, count);
                Array.Copy(bytes, offset, block, 0, blockLength);

                var run = (blockLength + 3) / 4;
                for (int i = 0, j = 0; j < run; i += 4, j++)
                {
                    hash = (hash >> 1) + BitConverter.ToUInt32(block, i) + (hash << 31);
                }

                offset += blockLength;
                count -= blockLength;
            }
            return hash;
        }

        public static uint Bogocrypt1(byte[] bytes, int offset, int count, uint key)
        {
            var end = offset + count;
            for (var i = offset; i < end; i += 4)
            {
                bytes[i + 0] ^= (byte)((key >> 0) & 0xFF);
                bytes[i + 1] ^= (byte)((key >> 8) & 0xFF);
                bytes[i + 2] ^= (byte)((key >> 16) & 0xFF);
                bytes[i + 3] ^= (byte)((key >> 24) & 0xFF);

                switch (key & 15)
                {
                    case 2:
                    {
                        SwapBytes(bytes, i + 0, i + 3);
                        SwapBytes(bytes, i + 1, i + 2);
                        break;
                    }

                    case 9:
                    {
                        SwapBytes(bytes, i + 0, i + 1);
                        SwapBytes(bytes, i + 2, i + 3);
                        break;
                    }

                    case 13:
                    {
                        SwapBytes(bytes, i + 0, i + 2);
                        SwapBytes(bytes, i + 1, i + 3);
                        break;
                    }
                }

                key ^= ((key ^ (key << 8)) >> 9) ^ (key << 8) ^ ((((key ^ (key << 8)) >> 9) ^ key ^ (key << 8)) << 23);
            }
            return key;
        }

        public static void Bogocrypt2(byte[] bytes, int offset, int count)
        {
            var table = _Bogocrypt2Table;
            for (int i = 0, o = offset; i < count; i += 4, o += 4)
            {
                bytes[o + 0] ^= table[(i + 0) & 0x7F];
                bytes[o + 1] ^= table[(i + 1) & 0x7F];
                bytes[o + 2] ^= table[(i + 2) & 0x7F];
                bytes[o + 3] ^= table[(i + 3) & 0x7F];
            }
        }

        private static byte[] GetBogocrypt2Table()
        {
            return new byte[]
            {
                0xB0, 0x22, 0x5D, 0x47, 0xD3, 0x1B, 0x59, 0xD1,
                0x97, 0x37, 0xFB, 0xD2, 0xC0, 0x31, 0x86, 0xFD,
                0xB5, 0xDD, 0xAC, 0x4B, 0xB4, 0x01, 0x15, 0x52,
                0x01, 0x0A, 0x75, 0x08, 0x5D, 0x50, 0xFE, 0x8C,
                0xA8, 0xAF, 0xD5, 0xCB, 0x09, 0xEE, 0xDC, 0xCA,
                0x76, 0xC4, 0x3F, 0x67, 0xED, 0x09, 0x8F, 0x64,
                0x43, 0x56, 0x30, 0x1A, 0x89, 0x2E, 0xDF, 0xF9,
                0xE4, 0xB8, 0x13, 0x1A, 0xBE, 0xAF, 0xA4, 0x02,
                0xFF, 0x60, 0x22, 0xE0, 0x0B, 0xFF, 0xAE, 0xC9,
                0x7D, 0xA0, 0x27, 0xF9, 0x11, 0x63, 0x2F, 0xE1,
                0x20, 0x7E, 0x51, 0xD7, 0x0E, 0xCD, 0xC4, 0xD4,
                0xCF, 0xCF, 0xA1, 0x88, 0x99, 0x42, 0x96, 0xCF,
                0xF0, 0xC1, 0x54, 0xC3, 0x36, 0x94, 0xC2, 0x92,
                0xDC, 0xB8, 0x63, 0xCA, 0xCA, 0x32, 0x12, 0x7A,
                0x3C, 0x88, 0x5F, 0xD2, 0x4B, 0xC3, 0xE4, 0x69,
                0x74, 0x32, 0xBD, 0x2C, 0x42, 0x28, 0xB2, 0x34,
            };
        }

        private static void SwapBytes(byte[] bytes, int left, int right)
        {
            var temp = bytes[left];
            bytes[left] = bytes[right];
            bytes[right] = temp;
        }
    }
}
