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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.ProjectData;
using Gibbed.Rebirth.FileFormats;
using NDesk.Options;

namespace RebuildAnimationLists
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private static string GetListPath(string installPath, string inputPath)
        {
            installPath = installPath.ToLowerInvariant();
            inputPath = inputPath.ToLowerInvariant();

            if (inputPath.StartsWith(installPath) == false)
            {
                return null;
            }

            var baseName = inputPath.Substring(installPath.Length + 1);

            string outputPath;
            outputPath = Path.Combine("files", baseName);
            outputPath = Path.ChangeExtension(outputPath, ".animlist");
            return outputPath;
        }

        public static void Main(string[] args)
        {
            bool showHelp = false;
            string currentProject = null;

            var options = new OptionSet()
            {
                { "h|help", "show this message and exit", v => showHelp = v != null },
                { "p|project=", "override current project", v => currentProject = v },
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

            if (extras.Count != 0 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            Console.WriteLine("Loading project...");

            var manager = Manager.Load(currentProject);
            if (manager.ActiveProject == null)
            {
                Console.WriteLine("Nothing to do: no active project loaded.");
                return;
            }

            var project = manager.ActiveProject;

            var installPath = project.InstallPath;
            var listsPath = project.ListsPath;

            if (installPath == null)
            {
                Console.WriteLine("Could not detect install path.");
                return;
            }

            if (listsPath == null)
            {
                Console.WriteLine("Could not detect lists path.");
                return;
            }

            var knownHashes = manager.LoadListsAnimationNames();

            var archivePaths = new List<string>();
            archivePaths.Add(Path.Combine(installPath, "afterbirth.a"));
            archivePaths.Add(Path.Combine(installPath, "animations.a"));

            var outputPaths = new List<string>();

            var nameHash = ArchiveFile.ComputeNameHash("resources/animations.b");
            for (int i = 0; i < archivePaths.Count; i++)
            {
                var archivePath = archivePaths[i];
                if (File.Exists(archivePath) == false)
                {
                    continue;
                }

                var outputPath = GetListPath(installPath, archivePath);
                if (outputPath == null)
                {
                    throw new InvalidOperationException();
                }

                Console.WriteLine(outputPath);
                outputPath = Path.Combine(listsPath, outputPath);

                if (outputPaths.Contains(outputPath) == true)
                {
                    throw new InvalidOperationException();
                }

                outputPaths.Add(outputPath);

                if (File.Exists(archivePath + ".bak") == true)
                {
                    archivePath += ".bak";
                }

                var hashes = new List<uint>();

                using (var input = File.OpenRead(archivePath))
                {
                    IArchiveFile archive;

                    if (ArchiveFile.IsValid(input) == true)
                    {
                        archive = new ArchiveFile();
                    }
                    else if (Gibbed.Antibirth.FileFormats.ArchiveFile.IsValid(input) == true)
                    {
                        archive = new Gibbed.Antibirth.FileFormats.ArchiveFile();
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }

                    archive.Deserialize(input);

                    var entry = archive.Entries.FirstOrDefault(e => e.NameHash == nameHash);
                    if (entry == null)
                    {
                        continue;
                    }

                    input.Seek(entry.Offset, SeekOrigin.Begin);
                    using (var data = entry.Read(input, archive))
                    {
                        var cache = new AnimationCacheBinaryFile();
                        cache.Deserialize(data);
                        hashes.AddRange(cache.AnimatedActors.Keys);
                    }
                }

                HandleEntries(hashes, knownHashes, outputPath);
            }
        }

        private static void HandleEntries(IEnumerable<uint> hashes,
                                          HashList<uint> knownHashes,
                                          string outputPath)
        {
            var breakdown = new Breakdown();

            var names = new List<string>();
            foreach (var hash in hashes)
            {
                var name = knownHashes[hash];
                if (name != null)
                {
                    names.Add(name);
                }

                breakdown.Total++;
            }

            var distinctNames = names.Distinct().ToArray();
            breakdown.Known += distinctNames.Length;

            var outputParent = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(outputParent) == false)
            {
                Directory.CreateDirectory(outputParent);
            }

            using (var writer = new StringWriter())
            {
                writer.WriteLine("; {0}", breakdown);

                foreach (string name in distinctNames.OrderBy(dn => dn))
                {
                    writer.WriteLine(name);
                }

                writer.Flush();

                using (var output = new StreamWriter(outputPath))
                {
                    output.Write(writer.GetStringBuilder());
                }
            }
        }

        private static void Bogocrypt1(ArchiveEntry entry, Stream input, Stream output)
        {
            var key = entry.BogocryptKey;
            long remaining = entry.Length;

            var block = new byte[1024];
            while (remaining > 0)
            {
                var blockLength = (int)Math.Min(block.Length, remaining + 3 & ~3);
                var actualBlockLength = (int)Math.Min(block.Length, remaining);
                if (blockLength == 0)
                {
                    throw new InvalidOperationException();
                }

                if (input.Read(block, 0, blockLength) < actualBlockLength)
                {
                    throw new EndOfStreamException();
                }

                key = ArchiveFile.Bogocrypt1(block, 0, blockLength, key);

                output.Write(block, 0, actualBlockLength);
                remaining -= blockLength;
            }
        }
    }
}
