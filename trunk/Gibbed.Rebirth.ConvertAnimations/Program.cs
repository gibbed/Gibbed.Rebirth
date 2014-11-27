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

namespace Gibbed.Rebirth.ConvertAnimations
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
            bool verbose = false;

            var options = new OptionSet()
            {
                { "binary", "convert XML to binary", v => mode = v != null ? Mode.ToBinary : mode },
                { "xml", "convert binary to XML", v => mode = v != null ? Mode.ToXml : mode },
                { "v|verbose", "be verbose", v => verbose = v != null },
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

                if (extension == ".b")
                {
                    mode = Mode.ToXml;
                }
                else if (extension == ".anm2" || Directory.Exists(extras[0]) == true)
                {
                    mode = Mode.ToBinary;
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

            if (mode == Mode.ToBinary)
            {
                throw new NotImplementedException();
            }
            else if (mode == Mode.ToXml)
            {
                string inputPath = extras[0];
                string outputPath = extras.Count > 1
                                        ? extras[1]
                                        : Path.ChangeExtension(inputPath, null) + "_converted";

                var cache = new AnimationCacheBinaryFile();
                using (var input = File.OpenRead(inputPath))
                {
                    cache.Deserialize(input);
                }

                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    OmitXmlDeclaration = true,
                    Indent = true,
                    IndentChars = "  ",
                };

                var culture = CultureInfo.InvariantCulture;

                if (verbose == true)
                {
                    Console.WriteLine("Loading project...");
                }

                var manager = ProjectData.Manager.Load();
                if (manager.ActiveProject == null)
                {
                    Console.WriteLine("Warning: no active project loaded.");
                }

                var hashes = manager.LoadListsAnimationNames();

                foreach (var kv in cache.AnimatedActors)
                {
                    string entryName = hashes[kv.Key];

                    if (entryName == null)
                    {
                        entryName = kv.Key.ToString("X8") + ".anm2";
                        entryName = Path.Combine("__UNKNOWN", entryName);
                    }
                    else
                    {
                        entryName = FilterEntryName(entryName);
                    }

                    var entryPath = Path.Combine(outputPath, entryName);

                    var entryParent = Path.GetDirectoryName(entryPath);
                    if (string.IsNullOrEmpty(entryParent) == false)
                    {
                        Directory.CreateDirectory(entryParent);
                    }

                    using (var writer = XmlWriter.Create(entryPath, settings))
                    {
                        var instance = kv.Value;

                        writer.WriteStartDocument();
                        writer.WriteComment("Converted to ANM2 by Gibbed.Rebirth.ConvertAnimations");

                        writer.WriteStartElement("AnimatedActor");

                        writer.WriteStartElement("Content");

                        writer.WriteStartElement("Spritesheets");
                        writer.WriteAttributeString("BasePath", instance.Content.BaseSpritesheetPath);
                        foreach (var spritesheet in instance.Content.Spritesheets)
                        {
                            writer.WriteStartElement("Spritesheet");
                            writer.WriteAttributeString("Path", spritesheet.Path);
                            writer.WriteAttributeString("Id", spritesheet.Id.ToString(culture));
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();

                        writer.WriteStartElement("Layers");
                        foreach (var layer in instance.Content.Layers)
                        {
                            writer.WriteStartElement("Layer");
                            writer.WriteAttributeString("Name", layer.Name);
                            writer.WriteAttributeString("Id", layer.Id.ToString(culture));
                            writer.WriteAttributeString("SpritesheetId", layer.SpritesheetId.ToString(culture));
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();

                        writer.WriteStartElement("Nulls");
                        foreach (var null_ in instance.Content.Nulls)
                        {
                            writer.WriteStartElement("Null");
                            writer.WriteAttributeString("Id", null_.Id.ToString(culture));
                            writer.WriteAttributeString("Name", null_.Name);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();

                        writer.WriteStartElement("Events");
                        foreach (var event_ in instance.Content.Events)
                        {
                            writer.WriteStartElement("Event");
                            writer.WriteAttributeString("Id", event_.Id.ToString(culture));
                            writer.WriteAttributeString("Name", event_.Name);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();

                        writer.WriteEndElement();

                        writer.WriteStartElement("Animations");
                        writer.WriteAttributeString("DefaultAnimation", instance.DefaultAnimation);
                        foreach (var animation in instance.Animations)
                        {
                            writer.WriteStartElement("Animation");
                            writer.WriteAttributeString("Name", animation.Name);
                            writer.WriteAttributeString("FrameNum", animation.FrameNum.ToString(culture));
                            writer.WriteAttributeString("Loop", animation.Loop.ToString(culture));

                            writer.WriteStartElement("RootAnimation");
                            foreach (var frame in animation.RootAnimation.Frames)
                            {
                                writer.WriteStartElement("Frame");
                                WriteFrame(writer, frame, culture);
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();

                            writer.WriteStartElement("LayerAnimations");
                            foreach (var layerAnimation in animation.LayerAnimations)
                            {
                                writer.WriteStartElement("LayerAnimation");

                                writer.WriteAttributeString("LayerId", layerAnimation.LayerId.ToString(culture));
                                writer.WriteAttributeString("Visible", layerAnimation.Visible.ToString(culture));

                                foreach (var frame in layerAnimation.Frames)
                                {
                                    writer.WriteStartElement("Frame");
                                    WriteFrame(writer, frame, culture);
                                    writer.WriteEndElement();
                                }

                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();

                            writer.WriteStartElement("NullAnimations");
                            foreach (var nullAnimation in animation.NullAnimations)
                            {
                                writer.WriteStartElement("NullAnimation");

                                writer.WriteAttributeString("NullId", nullAnimation.NullId.ToString(culture));
                                writer.WriteAttributeString("Visible", nullAnimation.Visible.ToString(culture));

                                foreach (var frame in nullAnimation.Frames)
                                {
                                    writer.WriteStartElement("Frame");
                                    WriteFrame(writer, frame, culture);
                                    writer.WriteEndElement();
                                }

                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();

                            writer.WriteStartElement("Triggers");
                            foreach (var trigger in animation.Triggers)
                            {
                                writer.WriteStartElement("Trigger");
                                writer.WriteAttributeString("EventId", trigger.EventId.ToString(culture));
                                writer.WriteAttributeString("AtFrame", trigger.AtFrame.ToString(culture));
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();

                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static void WriteFrame(XmlWriter writer, FileFormats.Animation.NullFrame frame, CultureInfo culture)
        {
            writer.WriteAttributeString("XPosition", frame.XPosition.ToString(culture));
            writer.WriteAttributeString("YPosition", frame.YPosition.ToString(culture));
            writer.WriteAttributeString("Delay", frame.Delay.ToString(culture));
            writer.WriteAttributeString("Visible", frame.Visible.ToString(culture));
            writer.WriteAttributeString("XScale", frame.XScale.ToString(culture));
            writer.WriteAttributeString("YScale", frame.YScale.ToString(culture));
            writer.WriteAttributeString("RedTint", (frame.RedTint * 255.0f).ToString(culture));
            writer.WriteAttributeString("GreenTint", (frame.GreenTint * 255.0f).ToString(culture));
            writer.WriteAttributeString("BlueTint", (frame.BlueTint * 255.0f).ToString(culture));
            writer.WriteAttributeString("AlphaTint", (frame.AlphaTint * 255.0f).ToString(culture));
            writer.WriteAttributeString("RedOffset", (frame.RedOffset * 255.0f).ToString(culture));
            writer.WriteAttributeString("GreenOffset", (frame.GreenOffset * 255.0f).ToString(culture));
            writer.WriteAttributeString("BlueOffset", (frame.BlueOffset * 255.0f).ToString(culture));
            writer.WriteAttributeString("Rotation", frame.Rotation.ToString(culture));
            writer.WriteAttributeString("Interpolated", frame.Interpolated.ToString(culture));
        }

        private static void WriteFrame(XmlWriter writer, FileFormats.Animation.LayerFrame frame, CultureInfo culture)
        {
            writer.WriteAttributeString("XPosition", frame.XPosition.ToString(culture));
            writer.WriteAttributeString("YPosition", frame.YPosition.ToString(culture));
            writer.WriteAttributeString("XPivot", frame.XPivot.ToString(culture));
            writer.WriteAttributeString("YPivot", frame.YPivot.ToString(culture));
            writer.WriteAttributeString("Width", frame.Width.ToString(culture));
            writer.WriteAttributeString("Height", frame.Height.ToString(culture));
            writer.WriteAttributeString("XScale", frame.XScale.ToString(culture));
            writer.WriteAttributeString("YScale", frame.YScale.ToString(culture));
            writer.WriteAttributeString("Delay", frame.Delay.ToString(culture));
            writer.WriteAttributeString("Visible", frame.Visible.ToString(culture));
            writer.WriteAttributeString("XCrop", frame.XCrop.ToString(culture));
            writer.WriteAttributeString("YCrop", frame.YCrop.ToString(culture));
            writer.WriteAttributeString("RedTint", (frame.RedTint * 255.0f).ToString(culture));
            writer.WriteAttributeString("GreenTint", (frame.GreenTint * 255.0f).ToString(culture));
            writer.WriteAttributeString("BlueTint", (frame.BlueTint * 255.0f).ToString(culture));
            writer.WriteAttributeString("AlphaTint", (frame.AlphaTint * 255.0f).ToString(culture));
            writer.WriteAttributeString("RedOffset", (frame.RedOffset * 255.0f).ToString(culture));
            writer.WriteAttributeString("GreenOffset", (frame.GreenOffset * 255.0f).ToString(culture));
            writer.WriteAttributeString("BlueOffset", (frame.BlueOffset * 255.0f).ToString(culture));
            writer.WriteAttributeString("Rotation", frame.Rotation.ToString(culture));
            writer.WriteAttributeString("Interpolated", frame.Interpolated.ToString(culture));
        }

        private static string FilterEntryName(string entryName)
        {
            entryName = entryName.Replace('/', Path.DirectorySeparatorChar);
            if (entryName.Length > 0 && entryName[0] == Path.DirectorySeparatorChar)
            {
                entryName = entryName.Substring(1);
            }
            return entryName;
        }
    }
}
