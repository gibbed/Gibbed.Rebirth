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

using System.IO;
using Gibbed.IO;

namespace Gibbed.Rebirth.FileFormats.Animation
{
    public struct NullFrame
    {
        public float XPosition;
        public float YPosition;
        public int Delay;
        public bool Visible;
        public float XScale;
        public float YScale;
        public float RedTint;
        public float GreenTint;
        public float BlueTint;
        public float AlphaTint;
        public float RedOffset;
        public float GreenOffset;
        public float BlueOffset;
        public float Rotation;
        public bool Interpolated;

        internal static NullFrame Read(Stream input, Endian endian)
        {
            NullFrame instance;
            instance.XPosition = input.ReadValueF32(endian);
            instance.YPosition = input.ReadValueF32(endian);
            instance.XScale = input.ReadValueF32(endian);
            instance.YScale = input.ReadValueF32(endian);
            instance.Delay = input.ReadValueS32(endian);
            instance.Visible = input.ReadValueB8();
            instance.RedTint = input.ReadValueF32(endian);
            instance.GreenTint = input.ReadValueF32(endian);
            instance.BlueTint = input.ReadValueF32(endian);
            instance.AlphaTint = input.ReadValueF32(endian);
            instance.RedOffset = input.ReadValueF32(endian);
            instance.GreenOffset = input.ReadValueF32(endian);
            instance.BlueOffset = input.ReadValueF32(endian);
            instance.Rotation = input.ReadValueF32(endian);
            instance.Interpolated = input.ReadValueB8();
            return instance;
        }

        internal void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }

        internal static void Write(Stream output, NullFrame instance, Endian endian)
        {
            output.WriteValueF32(instance.XPosition, endian);
            output.WriteValueF32(instance.YPosition, endian);
            output.WriteValueF32(instance.XScale, endian);
            output.WriteValueF32(instance.YScale, endian);
            output.WriteValueS32(instance.Delay, endian);
            output.WriteValueB8(instance.Visible);
            output.WriteValueF32(instance.RedTint, endian);
            output.WriteValueF32(instance.GreenTint, endian);
            output.WriteValueF32(instance.BlueTint, endian);
            output.WriteValueF32(instance.AlphaTint, endian);
            output.WriteValueF32(instance.RedOffset, endian);
            output.WriteValueF32(instance.GreenOffset, endian);
            output.WriteValueF32(instance.BlueOffset, endian);
            output.WriteValueF32(instance.Rotation, endian);
            output.WriteValueB8(instance.Interpolated);
        }
    }
}
