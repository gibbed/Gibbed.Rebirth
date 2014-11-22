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
    public class AnimationBinaryFile
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
                var unknown0 = input.ReadValueU32(endian);
                var unknown1 = ReadString(input, endian);

                var unknown2 = input.ReadValueU32(endian);
                for (uint j = 0; j < unknown2; j++)
                {
                    var unknown3 = input.ReadValueU32(endian);
                    var unknown4 = ReadString(input, endian);
                }

                var unknown5 = input.ReadValueU32(endian);
                for (uint j = 0; j < unknown5; j++)
                {
                    var unknown6 = input.ReadValueU32(endian);
                    var unknown7 = input.ReadValueU32(endian);
                    var unknown8 = ReadString(input, endian);
                }

                var unknown9 = input.ReadValueU32(endian);
                for (uint j = 0; j < unknown9; j++)
                {
                    var unknown10 = input.ReadValueU32(endian);
                    var unknown11 = ReadString(input, endian);
                }

                var unknown12 = input.ReadValueU32(endian);
                for (uint j = 0; j < unknown12; j++)
                {
                    var unknown13 = input.ReadValueU32(endian);
                    var unknown14 = ReadString(input, endian);
                }

                var unknown15 = ReadString(input, endian);

                var unknown16 = input.ReadValueU32(endian);
                for (uint j = 0; j < unknown16; j++)
                {
                    var unknown17 = ReadString(input, endian);
                    var unknown18 = input.ReadValueU32(endian);
                    var unknown19 = input.ReadValueU8();

                    var unknown20 = input.ReadValueU32(endian);
                    for (uint k = 0; k < unknown20; k++)
                    {
                        var unknown21 = input.ReadValueU32(endian);
                        var unknown22 = input.ReadValueU32(endian);
                        var unknown23 = input.ReadValueU32(endian);
                        var unknown24 = input.ReadValueU32(endian);
                        var unknown25 = input.ReadValueU32(endian);
                        var unknown26 = input.ReadValueU8();

                        var unknown27 = input.ReadValueU32(endian);
                        var unknown28 = input.ReadValueU32(endian);
                        var unknown29 = input.ReadValueU32(endian);
                        var unknown30 = input.ReadValueU32(endian);

                        var unknown31 = input.ReadValueU32(endian);
                        var unknown32 = input.ReadValueU32(endian);
                        var unknown33 = input.ReadValueU32(endian);

                        var unknown34 = input.ReadValueU32(endian);
                        var unknown35 = input.ReadValueU8();
                    }

                    var unknown36 = input.ReadValueU32(endian);
                    for (uint k = 0; k < unknown36; k++)
                    {
                        var unknown37 = input.ReadValueU32(endian);
                        var unknown38 = input.ReadValueU8();

                        var unknown39 = input.ReadValueU32(endian);
                        for (uint l = 0; l < unknown39; l++)
                        {
                            var unknown40 = input.ReadValueU32(endian);
                            var unknown41 = input.ReadValueU32(endian);
                            var unknown42 = input.ReadValueU32(endian);
                            var unknown43 = input.ReadValueU32(endian);
                            var unknown44 = input.ReadValueU32(endian);
                            var unknown45 = input.ReadValueU32(endian);
                            var unknown46 = input.ReadValueU32(endian);
                            var unknown47 = input.ReadValueU32(endian);
                            var unknown48 = input.ReadValueU32(endian);
                            var unknown49 = input.ReadValueU32(endian);
                            var unknown50 = input.ReadValueU32(endian);
                            var unknown51 = input.ReadValueU8();

                            var unknown52 = input.ReadValueU32(endian);
                            var unknown53 = input.ReadValueU32(endian);
                            var unknown54 = input.ReadValueU32(endian);
                            var unknown55 = input.ReadValueU32(endian);

                            var unknown56 = input.ReadValueU32(endian);
                            var unknown57 = input.ReadValueU32(endian);
                            var unknown58 = input.ReadValueU32(endian);

                            var unknown59 = input.ReadValueU32(endian);
                            var unknown60 = input.ReadValueU8();
                        }
                    }

                    var unknown61 = input.ReadValueU32(endian);
                    for (uint k = 0; k < unknown61; k++)
                    {
                        var unknown62 = input.ReadValueU32(endian);
                        var unknown63 = input.ReadValueU8();

                        var unknown64 = input.ReadValueU32(endian);
                        for (uint l = 0; l < unknown64; l++)
                        {
                            var unknown21 = input.ReadValueU32(endian);
                            var unknown22 = input.ReadValueU32(endian);
                            var unknown23 = input.ReadValueU32(endian);
                            var unknown24 = input.ReadValueU32(endian);
                            var unknown25 = input.ReadValueU32(endian);
                            var unknown26 = input.ReadValueU8();

                            var unknown27 = input.ReadValueU32(endian);
                            var unknown28 = input.ReadValueU32(endian);
                            var unknown29 = input.ReadValueU32(endian);
                            var unknown30 = input.ReadValueU32(endian);

                            var unknown31 = input.ReadValueU32(endian);
                            var unknown32 = input.ReadValueU32(endian);
                            var unknown33 = input.ReadValueU32(endian);

                            var unknown34 = input.ReadValueU32(endian);
                            var unknown35 = input.ReadValueU8();
                        }
                    }

                    var unknown65 = input.ReadValueU32(endian);
                    for (uint k = 0; k < unknown65; k++)
                    {
                        var unknown66 = input.ReadValueU32(endian);
                        var unknown67 = input.ReadValueU32(endian);
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
