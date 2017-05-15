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
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Antibirth.FileFormats
{
    public class ArchiveFile : Rebirth.FileFormats.IArchiveFile
    {
        private const uint _Signature = 0x484E4956; // 'VINH'

        private long _BasePosition;
        private Endian _Endian;
        private Rebirth.FileFormats.ArchiveCompressionMode _CompressionMode;
        private readonly List<ArchiveEntry> _Entries;

        public ArchiveFile()
        {
            this._Entries = new List<ArchiveEntry>();
        }

        public bool HasChecksums
        {
            get { return false; }
        }

        public long BasePosition
        {
            get { return this._BasePosition; }
            set { this._BasePosition = value; }
        }

        public Endian Endian
        {
            get { return this._Endian; }
            set { this._Endian = value; }
        }

        public Rebirth.FileFormats.ArchiveCompressionMode CompressionMode
        {
            get { return this._CompressionMode; }
            set { this._CompressionMode = value; }
        }

        public IEnumerable<Rebirth.FileFormats.IArchiveEntry> Entries
        {
            get { return this._Entries; }
        }

        public static bool IsValid(Stream input)
        {
            const Endian endian = Endian.Little;
            var basePosition = input.Position;
            var magic = input.ReadValueU32(endian);
            input.Position = basePosition;
            return magic == _Signature;
        }

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            const Endian endian = Endian.Little;

            var basePosition = input.Position;

            var magic = input.ReadValueU32(endian);
            if (magic != _Signature)
            {
                throw new FormatException();
            }

            this._BasePosition = basePosition;
            this._Endian = endian;
            this._CompressionMode = Rebirth.FileFormats.ArchiveCompressionMode.LZW;
            this._Entries.Clear();

            long entryOffset = (long)(input.ReadValueU64(endian) ^ ArchiveConstants.FirstEntryOffsetXor);
            while (entryOffset != -1)
            {
                input.Position = basePosition + entryOffset;
                var isFile = input.ReadValueB8();
                /*var parentEntryOffset = (long)(input.ReadValueU64(endian) ^ ArchiveConstants.ParentEntryOffsetXor);
                var unknown9 = input.ReadValueU64(endian);*/
                input.Seek(16, SeekOrigin.Current);
                var nextEntryOffset = (long)(input.ReadValueU64(endian) ^ ArchiveConstants.NextEntryOffsetXor);
                var nameHash = input.ReadValueU64(endian);

                if (isFile == false)
                {
                    //var unknown21 = input.ReadValueU32(endian);
                    input.Seek(4, SeekOrigin.Current);
                }
                else
                {
                    var dataSize = input.ReadValueU32(endian) ^ ArchiveConstants.EntryDataSizeXor;
                    var dataMagic = input.ReadValueU32(endian) ^ ArchiveConstants.EntryDataMagicXor;
                    var chunkOffset = (long)(input.ReadValueU64(endian) ^ ArchiveConstants.FirstChunkOffsetXor);

                    var entry = new ArchiveEntry();
                    entry.NameHash = nameHash;
                    entry.Offset = chunkOffset;
                    entry.Length = dataSize;
                    entry.Magic = dataMagic;

                    var originalPosition = input.Position;
                    while (chunkOffset != -1)
                    {
                        entry.ChunkOffsets.Add(chunkOffset + 8);
                        input.Position = basePosition + chunkOffset;
                        var nextChunkOffset = (long)(input.ReadValueU64(endian) ^ ArchiveConstants.NextChunkOffsetXor);
                        chunkOffset = nextChunkOffset;
                    }
                    input.Position = originalPosition;

                    this._Entries.Add(entry);
                }

                entryOffset = nextEntryOffset;
            }
        }

        public static ulong ComputeNameHash(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            throw new NotImplementedException();
        }
    }
}
