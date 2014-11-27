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
using Gibbed.IO;

namespace Gibbed.Rebirth.FileFormats.Animation
{
    public struct Animation
    {
        public string Name;
        public uint FrameNum;
        public bool Loop;
        public RootAnimation RootAnimation;
        public LayerAnimation[] LayerAnimations;
        public NullAnimation[] NullAnimations;
        public Trigger[] Triggers;

        internal static Animation Read(Stream input, Endian endian)
        {
            var name = input.ReadString(endian);
            var frameNum = input.ReadValueU32(endian);
            var loop = input.ReadValueB8();
            var rootAnimation = RootAnimation.Read(input, endian);

            var layerAnimationCount = input.ReadValueU32(endian);
            if (layerAnimationCount >= 32)
            {
                throw new FormatException("too many animation layers");
            }
            var layerAnimations = new LayerAnimation[layerAnimationCount];
            for (uint i = 0; i < layerAnimationCount; i++)
            {
                layerAnimations[i] = LayerAnimation.Read(input, endian);
            }

            var nullAnimationCount = input.ReadValueU32(endian);
            if (nullAnimationCount >= 32)
            {
                throw new FormatException("too many null layers");
            }
            var nullAnimations = new NullAnimation[nullAnimationCount];
            for (uint k = 0; k < nullAnimationCount; k++)
            {
                nullAnimations[k] = NullAnimation.Read(input, endian);
            }

            var triggerCount = input.ReadValueU32(endian);
            if (triggerCount >= 32)
            {
                throw new FormatException("too many triggers");
            }
            var triggers = new Trigger[triggerCount];
            for (uint i = 0; i < triggerCount; i++)
            {
                triggers[i] = Trigger.Read(input, endian);
            }

            Animation instance;
            instance.Name = name;
            instance.FrameNum = frameNum;
            instance.Loop = loop;
            instance.RootAnimation = rootAnimation;
            instance.LayerAnimations = layerAnimations;
            instance.NullAnimations = nullAnimations;
            instance.Triggers = triggers;
            return instance;
        }

        internal void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }

        internal static void Write(Stream output, Animation instance, Endian endian)
        {
            output.WriteString(instance.Name, endian);
            output.WriteValueU32(instance.FrameNum, endian);
            output.WriteValueB8(instance.Loop);
            instance.RootAnimation.Write(output, endian);

            var layerAnimationCount = instance.LayerAnimations == null ? 0 : instance.LayerAnimations.Length;
            output.WriteValueS32(layerAnimationCount, endian);
            if (instance.LayerAnimations != null)
            {
                foreach (var layerAnimation in instance.LayerAnimations)
                {
                    layerAnimation.Write(output, endian);
                }
            }

            var nullAnimationCount = instance.NullAnimations == null ? 0 : instance.NullAnimations.Length;
            output.WriteValueS32(nullAnimationCount, endian);
            if (instance.NullAnimations != null)
            {
                foreach (var nullAnimation in instance.NullAnimations)
                {
                    nullAnimation.Write(output, endian);
                }
            }

            var triggerCount = instance.Triggers == null ? 0 : instance.Triggers.Length;
            output.WriteValueS32(triggerCount, endian);
            if (instance.Triggers != null)
            {
                foreach (var trigger in instance.Triggers)
                {
                    trigger.Write(output, endian);
                }
            }
        }
    }
}
