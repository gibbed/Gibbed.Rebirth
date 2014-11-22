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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Gibbed.Rebirth.FileFormats;
using NDesk.Options;

namespace Gibbed.Rebirth.ConvertStage
{
    public class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        public static void Main(string[] args)
        {
            var mode = Mode.Unknown;
            bool showHelp = false;

            var options = new OptionSet()
            {
                { "stb", "convert XML to STB", v => mode = v != null ? Mode.ToStb : mode },
                { "xml", "convert STB to XML", v => mode = v != null ? Mode.ToXml : mode },
                { "h|help", "show this message and exit", v => showHelp = v != null },
            };

            List<string> extras;

            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (mode == Mode.Unknown &&
                extras.Count >= 1)
            {
                var extension = Path.GetExtension(extras[0]);

                if (extension == ".stb")
                {
                    mode = Mode.ToXml;
                }
                else if (extension == ".xml")
                {
                    mode = Mode.ToStb;
                }
            }

            if (showHelp == true ||
                mode == Mode.Unknown ||
                extras.Count < 1 ||
                extras.Count > 2)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input [output]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            if (mode == Mode.ToStb)
            {
                string inputPath = extras[0];
                string outputPath = extras.Count > 1
                                        ? extras[1]
                                        : Path.ChangeExtension(inputPath, null) + "_converted.stb";

                var culture = CultureInfo.InvariantCulture;
                var stb = new StageBinaryFile();
                using (var input = File.OpenRead(inputPath))
                {
                    var doc = new XPathDocument(input);
                    var nav = doc.CreateNavigator();
                    var root = nav.SelectSingleNode("/stage");
                    if (root == null)
                    {
                        throw new InvalidOperationException();
                    }

                    var rawRooms = root.Select("room");
                    var rooms = new List<StageBinaryFile.Room>();
                    while (rawRooms.MoveNext() == true)
                    {
                        var rawRoom = rawRooms.Current;
                        var room = new StageBinaryFile.Room();
                        ParseAttribute(rawRoom, "type", out room.Type, culture);
                        ParseAttribute(rawRoom, "variant", out room.Variant, culture);
                        ParseAttribute(rawRoom, "name", out room.Name);
                        ParseAttribute(rawRoom, "difficulty", out room.Difficulty, culture);
                        ParseAttribute(rawRoom, "weight", out room.Weight, culture);
                        ParseAttribute(rawRoom, "width", out room.Width, culture);
                        ParseAttribute(rawRoom, "height", out room.Height, culture);

                        var rawDoors = rawRoom.Select("door");
                        var doors = new List<StageBinaryFile.Door>();
                        while (rawDoors.MoveNext() == true)
                        {
                            var rawDoor = rawDoors.Current;
                            var door = new StageBinaryFile.Door();
                            ParseAttribute(rawDoor, "x", out door.X, culture);
                            ParseAttribute(rawDoor, "y", out door.Y, culture);
                            ParseAttribute(rawDoor, "exists", out door.Exists);
                            doors.Add(door);
                        }
                        room.Doors = doors.ToArray();

                        var rawSpawns = rawRoom.Select("spawn");
                        var spawns = new List<StageBinaryFile.Spawn>();
                        while (rawSpawns.MoveNext() == true)
                        {
                            var rawSpawn = rawSpawns.Current;
                            var spawn = new StageBinaryFile.Spawn();
                            ParseAttribute(rawSpawn, "x", out spawn.X, culture);
                            ParseAttribute(rawSpawn, "y", out spawn.Y, culture);

                            var rawEntities = rawSpawn.Select("entity");
                            var entities = new List<StageBinaryFile.Entity>();
                            while (rawEntities.MoveNext() == true)
                            {
                                var rawEntity = rawEntities.Current;
                                var entity = new StageBinaryFile.Entity();
                                ParseAttribute(rawEntity, "type", out entity.Type, culture);
                                ParseAttribute(rawEntity, "variant", out entity.Variant, culture);
                                ParseAttribute(rawEntity, "subtype", out entity.Subtype, culture);
                                ParseAttribute(rawEntity, "weight", out entity.Weight, culture);
                                entities.Add(entity);
                            }
                            spawn.Entities = entities.ToArray();

                            spawns.Add(spawn);
                        }
                        room.Spawns = spawns.ToArray();
                        rooms.Add(room);
                    }

                    stb.Rooms.Clear();
                    stb.Rooms.AddRange(rooms);
                }

                using (var output = File.Create(outputPath))
                {
                    stb.Serialize(output);
                    output.Flush();
                }
            }
            else if (mode == Mode.ToXml)
            {
                string inputPath = extras[0];
                string outputPath = extras.Count > 1
                                        ? extras[1]
                                        : Path.ChangeExtension(inputPath, null) + "_converted.xml";

                var stb = new StageBinaryFile();
                using (var input = File.OpenRead(inputPath))
                {
                    stb.Deserialize(input);
                }

                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    OmitXmlDeclaration = true
                };

                var culture = CultureInfo.InvariantCulture;
                using (var writer = XmlWriter.Create(outputPath, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("stage");

                    foreach (var room in stb.Rooms)
                    {
                        writer.WriteStartElement("room");
                        writer.WriteAttributeString("type", room.Type.ToString(culture));
                        writer.WriteAttributeString("variant", room.Variant.ToString(culture));
                        writer.WriteAttributeString("name", room.Name);
                        writer.WriteAttributeString("difficulty", room.Difficulty.ToString(culture));
                        writer.WriteAttributeString("weight", room.Weight.ToString(culture));
                        writer.WriteAttributeString("width", room.Width.ToString(culture));
                        writer.WriteAttributeString("height", room.Height.ToString(culture));

                        foreach (var door in room.Doors)
                        {
                            writer.WriteStartElement("door");
                            writer.WriteAttributeString("x", door.X.ToString(culture));
                            writer.WriteAttributeString("y", door.Y.ToString(culture));

                            if (door.Exists == true)
                            {
                                writer.WriteAttributeString("exists", door.Exists.ToString(culture));
                            }

                            writer.WriteEndElement();
                        }

                        foreach (var spawn in room.Spawns)
                        {
                            writer.WriteStartElement("spawn");
                            writer.WriteAttributeString("x", spawn.X.ToString(culture));
                            writer.WriteAttributeString("y", spawn.Y.ToString(culture));

                            foreach (var entity in spawn.Entities)
                            {
                                writer.WriteStartElement("entity");
                                writer.WriteAttributeString("type", entity.Type.ToString(culture));
                                writer.WriteAttributeString("variant", entity.Variant.ToString(culture));
                                writer.WriteAttributeString("subtype", entity.Subtype.ToString(culture));
                                writer.WriteAttributeString("weight", entity.Weight.ToString(culture));
                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static void ParseAttribute(XPathNavigator nav, string name, out string value)
        {
            if (nav.MoveToAttribute(name, "") == false)
            {
                throw new KeyNotFoundException(string.Format("could not find attribute '{0}'", name));
            }

            value = nav.Value;
            nav.MoveToParent();
        }

        private static void ParseAttribute(XPathNavigator nav, string name, out bool value)
        {
            if (nav.MoveToAttribute(name, "") == false)
            {
                value = false;
                return;
            }

            if (bool.TryParse(nav.Value, out value) == false)
            {
                throw new FormatException(
                    string.Format("could not parse '{0}' as bool ({1})", name, nav.Value));
            }

            nav.MoveToParent();
        }

        private static void ParseAttribute<T>(XPathNavigator nav, string name, Func<string, bool> func)
        {
            if (nav.MoveToAttribute(name, "") == false)
            {
                throw new KeyNotFoundException(string.Format("could not find attribute '{0}'", name));
            }

            if (func(nav.Value) == false)
            {
                throw new FormatException(
                    string.Format("could not parse '{0}' as {1} ({2})", name, typeof(T).Name, nav.Value));
            }

            nav.MoveToParent();
        }

        private static void ParseAttribute(XPathNavigator nav, string name, out byte value, CultureInfo culture)
        {
            var dummy = default(byte);
            ParseAttribute<byte>(nav, name, t => byte.TryParse(t, NumberStyles.Integer, culture, out dummy));
            value = dummy;
        }

        private static void ParseAttribute(XPathNavigator nav, string name, out short value, CultureInfo culture)
        {
            var dummy = default(short);
            ParseAttribute<short>(nav, name, t => short.TryParse(t, NumberStyles.Integer, culture, out dummy));
            value = dummy;
        }

        private static void ParseAttribute(XPathNavigator nav, string name, out ushort value, CultureInfo culture)
        {
            var dummy = default(ushort);
            ParseAttribute<ushort>(nav, name, t => ushort.TryParse(t, NumberStyles.Integer, culture, out dummy));
            value = dummy;
        }

        private static void ParseAttribute(XPathNavigator nav, string name, out uint value, CultureInfo culture)
        {
            var dummy = default(uint);
            ParseAttribute<uint>(nav, name, t => uint.TryParse(t, NumberStyles.Integer, culture, out dummy));
            value = dummy;
        }

        private static void ParseAttribute(XPathNavigator nav, string name, out float value, CultureInfo culture)
        {
            var dummy = default(float);
            ParseAttribute<float>(nav, name, t => float.TryParse(t, NumberStyles.Float, culture, out dummy));
            value = dummy;
        }
    }
}
