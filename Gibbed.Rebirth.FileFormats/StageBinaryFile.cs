/* Copyright (c) 2014 Rick (rick 'at' gibbed 'dot' us)
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
using System.Text;
using Gibbed.IO;

namespace Gibbed.Rebirth.FileFormats
{
    public class StageBinaryFile
    {
        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            const Endian endian = Endian.Little;

            var count = input.ReadValueU32(endian);
            for (uint i = 0; i < count; i++)
            {
                var type = input.ReadValueU32(endian);
                var variant = input.ReadValueU32(endian);
                var difficulty = input.ReadValueU8();
                var name = ReadString(input, endian);
                var weight = input.ReadValueF32(endian);
                var width = input.ReadValueU8();
                var height = input.ReadValueU8();
                var doorCount = input.ReadValueU8();
                var spawnCount = input.ReadValueU16(endian);

                for (int j = 0; j < doorCount; j++)
                {
                    var x = input.ReadValueU16(endian);
                    var y = input.ReadValueU16(endian);
                    var exists = input.ReadValueU8();
                }

                for (int j = 0; j < spawnCount; j++)
                {
                    var x = input.ReadValueU16(endian);
                    var y = input.ReadValueU16(endian);

                    var entityCount = input.ReadValueU8();
                    for (int k = 0; k < entityCount; k++)
                    {
                        var entityType = input.ReadValueU16(endian);
                        var entityVariant = input.ReadValueU16(endian);
                        var entitySubtype = input.ReadValueU16(endian);
                        var entityWeight = input.ReadValueF32(endian);
                    }
                }
            }
        }

        private static string ReadString(Stream input, Endian endian)
        {
            var length = input.ReadValueU16(endian);
            var text = input.ReadString(length, true, Encoding.ASCII);
            return text;
        }
    }
}
