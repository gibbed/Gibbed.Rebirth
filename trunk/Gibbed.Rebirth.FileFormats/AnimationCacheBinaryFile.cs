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
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Rebirth.FileFormats
{
    public class AnimationCacheBinaryFile
    {
        private readonly Dictionary<uint, Animation.AnimatedActor> _AnimatedActors;

        public Dictionary<uint, Animation.AnimatedActor> AnimatedActors
        {
            get { return this._AnimatedActors; }
        }

        public AnimationCacheBinaryFile()
        {
            this._AnimatedActors = new Dictionary<uint, Animation.AnimatedActor>();
        }

        public void Serialize(Stream output)
        {
            const Endian endian = Endian.Little;

            output.WriteValueS32(this._AnimatedActors.Count, endian);
            foreach (var kv in this._AnimatedActors)
            {
                output.WriteValueU32(kv.Key, endian);
                kv.Value.Write(output, endian);
            }
        }

        public void Deserialize(Stream input)
        {
            const Endian endian = Endian.Little;

            var animatedActorCount = input.ReadValueU32(endian);
            this._AnimatedActors.Clear();
            for (uint i = 0; i < animatedActorCount; i++)
            {
                var nameHash = input.ReadValueU32(endian);
                var animatedActor = Animation.AnimatedActor.Read(input, endian);
                this._AnimatedActors.Add(nameHash, animatedActor);
            }
        }

        public static uint ComputeNameHash(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            uint hash = 5381;
            foreach (var c in path)
            {
                hash *= 33;
                hash += (byte)c;
            }
            return hash;
        }
    }
}
