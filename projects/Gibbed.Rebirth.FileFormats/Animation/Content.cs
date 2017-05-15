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
using Gibbed.IO;

namespace Gibbed.Rebirth.FileFormats.Animation
{
    public struct Content
    {
        public string BaseSpritesheetPath;
        public Spritesheet[] Spritesheets;
        public Layer[] Layers;
        public Null[] Nulls;
        public Event[] Events;

        internal static Content Read(Stream input, Endian endian)
        {
            var baseSpritesheetPath = input.ReadString(endian);

            var spritesheetCount = input.ReadValueU32(endian);
            var spritesheets = new Spritesheet[spritesheetCount];
            for (uint i = 0; i < spritesheetCount; i++)
            {
                var spritesheet = ReadSpritesheet(input, endian);
                if (spritesheet.Id >= 32 || spritesheet.Id != i)
                {
                    throw new FormatException("invalid spritesheet ID");
                }
                spritesheets[i] = spritesheet;
            }

            var layerCount = input.ReadValueU32(endian);
            var layers = new Layer[layerCount];
            for (uint i = 0; i < layerCount; i++)
            {
                var layer = ReadLayer(input, endian);

                if (layer.Id >= 32 || layer.Id != i)
                {
                    throw new FormatException("invalid layer ID");
                }

                if (layer.SpritesheetId >= spritesheetCount)
                {
                    throw new FormatException("invalid spritesheet ID");
                }

                layers[i] = layer;
            }

            if (layerCount == 0)
            {
                throw new FormatException("no layers");
            }

            var nullCount = input.ReadValueU32(endian);
            var nulls = new Null[nullCount];
            for (uint i = 0; i < nullCount; i++)
            {
                var null_ = ReadNull(input, endian);
                if (null_.Id >= 32 || null_.Id != i)
                {
                    throw new FormatException("invalid Null ID");
                }
                nulls[i] = null_;
            }

            var eventCount = input.ReadValueU32(endian);
            var events = new Event[eventCount];
            for (uint i = 0; i < eventCount; i++)
            {
                var event_ = ReadEvent(input, endian);
                if (event_.Id >= 16 || event_.Id != i)
                {
                    throw new FormatException("invalid Event ID");
                }
                events[i] = event_;
            }

            Content instance;
            instance.BaseSpritesheetPath = baseSpritesheetPath;
            instance.Spritesheets = spritesheets;
            instance.Layers = layers;
            instance.Nulls = nulls;
            instance.Events = events;
            return instance;
        }

        internal void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }

        internal static void Write(Stream output, Content instance, Endian endian)
        {
            var spritesheetCount = instance.Spritesheets == null ? 0 : instance.Spritesheets.Length;
            output.WriteValueS32(spritesheetCount, endian);
            if (instance.Spritesheets != null)
            {
                foreach (var spritesheet in instance.Spritesheets)
                {
                    WriteSpritesheet(output, spritesheet, endian);
                }
            }

            var layerCount = instance.Layers == null ? 0 : instance.Layers.Length;
            output.WriteValueS32(layerCount, endian);
            if (instance.Layers != null)
            {
                foreach (var layer in instance.Layers)
                {
                    WriteLayer(output, layer, endian);
                }
            }

            var nullCount = instance.Nulls == null ? 0 : instance.Nulls.Length;
            output.WriteValueS32(nullCount, endian);
            if (instance.Nulls != null)
            {
                foreach (var null_ in instance.Nulls)
                {
                    WriteNull(output, null_, endian);
                }
            }

            var eventCount = instance.Events == null ? 0 : instance.Events.Length;
            output.WriteValueS32(eventCount, endian);
            if (instance.Events != null)
            {
                foreach (var event_ in instance.Events)
                {
                    WriteEvent(output, event_, endian);
                }
            }
        }

        public struct Spritesheet
        {
            public string Path;
            public uint Id;
        }

        private static Spritesheet ReadSpritesheet(Stream input, Endian endian)
        {
            var instance = new Spritesheet();
            instance.Id = input.ReadValueU32(endian);
            instance.Path = input.ReadString(endian);
            return instance;
        }

        private static void WriteSpritesheet(Stream output, Spritesheet instance, Endian endian)
        {
            output.WriteValueU32(instance.Id, endian);
            output.WriteString(instance.Path, endian);
        }

        public struct Layer
        {
            public string Name;
            public uint Id;
            public uint SpritesheetId;
        }

        private static Layer ReadLayer(Stream input, Endian endian)
        {
            var instance = new Layer();
            instance.Id = input.ReadValueU32(endian);
            instance.SpritesheetId = input.ReadValueU32(endian);
            instance.Name = input.ReadString(endian);
            return instance;
        }

        private static void WriteLayer(Stream output, Layer instance, Endian endian)
        {
            output.WriteValueU32(instance.Id, endian);
            output.WriteValueU32(instance.SpritesheetId, endian);
            output.WriteString(instance.Name, endian);
        }

        public struct Null
        {
            public uint Id;
            public string Name;
        }

        private static Null ReadNull(Stream input, Endian endian)
        {
            var instance = new Null();
            instance.Id = input.ReadValueU32(endian);
            instance.Name = input.ReadString(endian);
            return instance;
        }

        private static void WriteNull(Stream output, Null instance, Endian endian)
        {
            output.WriteValueU32(instance.Id, endian);
            output.WriteString(instance.Name, endian);
        }

        public struct Event
        {
            public uint Id;
            public string Name;
        }

        private static Event ReadEvent(Stream input, Endian endian)
        {
            var instance = new Event();
            instance.Id = input.ReadValueU32(endian);
            instance.Name = input.ReadString(endian);
            return instance;
        }

        private static void WriteEvent(Stream output, Event instance, Endian endian)
        {
            output.WriteValueU32(instance.Id, endian);
            output.WriteString(instance.Name, endian);
        }
    }
}
