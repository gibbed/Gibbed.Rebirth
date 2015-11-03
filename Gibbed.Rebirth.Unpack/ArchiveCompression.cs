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
using System.IO;
using Gibbed.IO;
using Gibbed.Rebirth.FileFormats;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Gibbed.Rebirth.Unpack
{
    public static class ArchiveCompression
    {
        public static MemoryStream ReadEntry(
            Stream input,
            ArchiveFile.Entry entry,
            ArchiveCompressionMode mode,
            Endian endian)
        {
            switch (mode)
            {
                case ArchiveCompressionMode.Bogocrypt1:
                {
                    var output = new MemoryStream();
                    var key = entry.BogocryptKey;
                    long remaining = entry.Length;
                    var block = new byte[1024];
                    while (remaining > 0)
                    {
                        var blockLength = (int)Math.Min(block.Length, remaining + 3 & ~3);
                        var actualBlockLength = (int)Math.Min(block.Length, remaining);
                        if (blockLength == 0)
                        {
                            throw new InvalidOperationException();
                        }

                        if (input.Read(block, 0, blockLength) < actualBlockLength)
                        {
                            throw new EndOfStreamException();
                        }

                        key = ArchiveFile.Bogocrypt1(block, 0, blockLength, key);

                        output.Write(block, 0, actualBlockLength);
                        remaining -= blockLength;
                    }
                    output.Position = 0;
                    return output;
                }

                case ArchiveCompressionMode.LZW:
                {
                    var output = new MemoryStream();
                    LZW.Decompress(input, entry.Length, output, endian);
                    output.Position = 0;
                    return output;
                }

                case ArchiveCompressionMode.MiniZ:
                {
                    ISAAC isaac = null;
                    var outputBytes = new byte[entry.Length];
                    var outputOffset = 0;
                    var blockBytes = new byte[0x800];
                    long remaining = entry.Length;

                    bool isCompressed = true;
                    bool isLastBlock;
                    do
                    {
                        var blockFlags = input.ReadValueU32(endian);
                        var blockLength = (int)(blockFlags & ~0x80000000u);
                        isLastBlock = (blockFlags & 0x80000000u) != 0;

                        if (blockLength > blockBytes.Length)
                        {
                            throw new InvalidOperationException();
                        }

                        if (input.Read(blockBytes, 0, blockLength) != blockLength)
                        {
                            throw new EndOfStreamException();
                        }

                        if (isCompressed == false || (isLastBlock == false && blockLength == 1024))
                        {
                            isCompressed = false;
                            if (isaac == null)
                            {
                                isaac = entry.GetISAAC();
                            }

                            int seed = 0;
                            for (int o = 0; o < blockLength; o++)
                            {
                                if ((o & 3) != 0)
                                {
                                    seed >>= 8;
                                }
                                else
                                {
                                    seed = isaac.Value();
                                }

                                blockBytes[o] ^= (byte)seed;
                            }

                            Array.Copy(blockBytes, 0, outputBytes, outputOffset, blockLength);

                            outputOffset += blockLength;
                            remaining -= blockLength;
                        }
                        else
                        {
                            using (var temp = new MemoryStream(blockBytes, false))
                            {
                                var zlib = new InflaterInputStream(temp, new Inflater(true));
                                var read = zlib.Read(outputBytes, outputOffset, (int)Math.Min(remaining, 1024));
                                outputOffset += read;
                                remaining -= read;
                            }
                        }
                    }
                    while (isLastBlock == false);

                    return new MemoryStream(outputBytes);
                }

                case ArchiveCompressionMode.Bogocrypt2:
                {
                    var blockBytes = new byte[1024];

                    var output = new MemoryStream();
                    long remaining = entry.Length;
                    while (remaining >= 4)
                    {
                        var blockLength = (int)Math.Min(blockBytes.Length, remaining);
                        if (input.Read(blockBytes, 0, blockLength) != blockLength)
                        {
                            throw new EndOfStreamException();
                        }

                        ArchiveFile.Bogocrypt2(blockBytes, 0, blockLength);
                        output.Write(blockBytes, 0, blockLength);
                        remaining -= blockLength;
                    }

                    if (remaining > 0)
                    {
                        output.WriteFromStream(input, remaining);
                    }

                    output.Position = 0;
                    return output;
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
