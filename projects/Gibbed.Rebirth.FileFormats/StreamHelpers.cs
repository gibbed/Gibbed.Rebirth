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
using System.Text;
using Gibbed.IO;

namespace Gibbed.Rebirth.FileFormats
{
    internal static class StreamHelpers
    {
        public static string ReadString(this Stream stream, Endian endian)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var length = stream.ReadValueU16(endian);
            return stream.ReadString(length, true, Encoding.ASCII);
        }

        public static void WriteString(this Stream stream, string value, Endian endian)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var length = value.Length;
            if (length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException("value", "string is too long");
            }

            stream.WriteValueU16((ushort)length, endian);
            stream.WriteString(value, (uint)length, Encoding.ASCII);
        }
    }
}
