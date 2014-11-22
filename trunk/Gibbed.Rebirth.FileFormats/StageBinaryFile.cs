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
                var unknown2 = input.ReadValueU8();
                var name = ReadString(input, endian);
                var unknown4 = input.ReadValueU32(endian);
                var unknown5 = input.ReadValueU8();
                var unknown6 = input.ReadValueU8();
                var unknown7 = input.ReadValueU8();
                var unknown8 = input.ReadValueU16(endian);

                for (int j = 0; j < unknown7; j++)
                {
                    var unknown9 = input.ReadValueU16(endian);
                    var unknown10 = input.ReadValueU16(endian);
                    var unknown11 = input.ReadValueU8();
                }

                for (int j = 0; j < unknown8; j++)
                {
                    var unknown12 = input.ReadValueU16(endian);
                    var unknown13 = input.ReadValueU16(endian);

                    var unknown14 = input.ReadValueU8();
                    for (int k = 0; k < unknown14; k++)
                    {
                        var unknown15 = input.ReadValueU16(endian);
                        var unknown16 = input.ReadValueU16(endian);
                        var unknown17 = input.ReadValueU16(endian);
                        var unknown18 = input.ReadValueU32(endian);
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
