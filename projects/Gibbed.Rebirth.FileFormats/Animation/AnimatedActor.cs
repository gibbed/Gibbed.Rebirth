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

using System.IO;
using Gibbed.IO;

namespace Gibbed.Rebirth.FileFormats.Animation
{
    public struct AnimatedActor
    {
        public Content Content;
        public string DefaultAnimation;
        public Animation[] Animations;

        public static AnimatedActor Read(Stream input, Endian endian)
        {
            var content = Content.Read(input, endian);
            var defaultAnimation = input.ReadString(endian);
            var animationCount = input.ReadValueU32(endian);
            var animations = new Animation[animationCount];
            for (uint i = 0; i < animationCount; i++)
            {
                animations[i] = Animation.Read(input, endian);
            }

            AnimatedActor instance;
            instance.Content = content;
            instance.DefaultAnimation = defaultAnimation;
            instance.Animations = animations;
            return instance;
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }

        public static void Write(Stream output, AnimatedActor instance, Endian endian)
        {
            instance.Content.Write(output, endian);
            output.WriteString(instance.DefaultAnimation, endian);
            var animationCount = instance.Animations == null ? 0 : instance.Animations.Length;
            output.WriteValueS32(animationCount, endian);
            if (instance.Animations != null)
            {
                foreach (var animation in instance.Animations)
                {
                    animation.Write(output, endian);
                }
            }
        }
    }
}
