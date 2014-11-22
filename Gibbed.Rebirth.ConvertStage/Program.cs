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

                throw new NotImplementedException();
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

                using (var writer = XmlWriter.Create(outputPath, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("stage");

                    foreach (var room in stb.Rooms)
                    {
                        writer.WriteStartElement("room");
                        writer.WriteAttributeString("type", room.Type.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString("variant", room.Variant.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString("name", room.Name);
                        writer.WriteAttributeString("difficulty", room.Difficulty.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString("weight", room.Weight.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString("width", room.Width.ToString(CultureInfo.InvariantCulture));
                        writer.WriteAttributeString("height", room.Height.ToString(CultureInfo.InvariantCulture));

                        foreach (var door in room.Doors)
                        {
                            writer.WriteStartElement("door");
                            writer.WriteAttributeString("x", door.X.ToString(CultureInfo.InvariantCulture));
                            writer.WriteAttributeString("y", door.Y.ToString(CultureInfo.InvariantCulture));
                            writer.WriteAttributeString("exists", door.Exists.ToString(CultureInfo.InvariantCulture));
                            writer.WriteEndElement();
                        }

                        foreach (var spawn in room.Spawns)
                        {
                            writer.WriteStartElement("spawn");
                            writer.WriteAttributeString("x", spawn.X.ToString(CultureInfo.InvariantCulture));
                            writer.WriteAttributeString("y", spawn.Y.ToString(CultureInfo.InvariantCulture));

                            foreach (var entity in spawn.Entities)
                            {
                                writer.WriteStartElement("entity");
                                writer.WriteAttributeString("type", entity.Type.ToString(CultureInfo.InvariantCulture));
                                writer.WriteAttributeString("variant",
                                                            entity.Variant.ToString(CultureInfo.InvariantCulture));
                                writer.WriteAttributeString("subtype",
                                                            entity.Subtype.ToString(CultureInfo.InvariantCulture));
                                writer.WriteAttributeString("weight",
                                                            entity.Weight.ToString(CultureInfo.InvariantCulture));
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
    }
}
