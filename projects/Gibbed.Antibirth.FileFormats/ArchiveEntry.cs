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
using Gibbed.IO;
using System.Collections.Generic;
using IArchiveEntry = Gibbed.Rebirth.FileFormats.IArchiveEntry;
using IArchiveFile = Gibbed.Rebirth.FileFormats.IArchiveFile;

namespace Gibbed.Antibirth.FileFormats
{
    public class ArchiveEntry : IArchiveEntry
    {
        public ulong NameHash;
        public long Offset;
        public uint Length;
        public uint Magic;
        public readonly List<long> ChunkOffsets;

        public ArchiveEntry()
        {
            this.ChunkOffsets = new List<long>();
        }

        ulong IArchiveEntry.NameHash
        {
            get { return this.NameHash; }
        }

        long IArchiveEntry.Offset
        {
            get { return this.Offset; }
        }

        uint IArchiveEntry.Length
        {
            get { return this.Length; }
        }

        uint IArchiveEntry.Checksum
        {
            get { throw new NotSupportedException(); }
        }

        MemoryStream IArchiveEntry.Read(Stream input, IArchiveFile archive)
        {
            var buffer = new byte[this.Length];

            var magic = (byte)(this.Magic & 0xFF);

            long remaining = this.Length;
            var o = 0;
            foreach (var chunkOffset in this.ChunkOffsets)
            {
                input.Position = archive.BasePosition + chunkOffset;

                var chunkSize = (int)Math.Min(4048, remaining);
                var read = input.Read(buffer, o, chunkSize);
                if (read != chunkSize)
                {
                    throw new EndOfStreamException();
                }

                Bogocrypt(buffer, o, chunkSize, magic);

                o += chunkSize;
                remaining -= chunkSize;
            }

            return new MemoryStream(buffer);
        }

        private static void Bogocrypt(byte[] buffer, int offset, int length, byte magic)
        {
            for (int i = 0, o = offset; i < length; i++, o++)
            {
                var o8 = (byte)(o % 8);
                var o16 = (byte)(o % 16);

                var m = magic;
                m <<= o16;
                m &= 0xF;
                m ^= o16;
                m += (byte)(o & 0xFF);
                m <<= 3;
                m = m.RotateLeft(o8);
                m ^= magic;

                var b = buffer[o];
                b ^= m;
                b = b.RotateRight(m % 5);
                b ^= m;
                buffer[o] = b;
            }
        }

        public const uint InitialHash32 = 0x811C9DC5;

        public static uint Hash32(string value)
        {
            var bytes = System.Text.Encoding.GetEncoding(1252).GetBytes(value);
            return Hash32(bytes, 0, bytes.Length);
        }

        public static uint Hash32(byte[] buffer, int offset, int count)
        {
            return Hash32(buffer, offset, count, InitialHash32);
        }

        public static uint Hash32(byte[] buffer, int offset, int count, uint hash)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            hash ^= (uint)count;
            hash >>= 16;


            for (int i = offset; i < offset + count; i++)
            {
                hash *= 0x1000193;
                hash ^= buffer[i];
            }

            return hash;
        }
    }
}
