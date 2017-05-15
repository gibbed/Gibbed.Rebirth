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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gibbed.IO;
using Gibbed.Rebirth.FileFormats;
using NDesk.Options;

namespace Gibbed.Rebirth.Unpack
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        public static void Main(string[] args)
        {
            bool showHelp = false;
            bool extractUnknowns = true;
            bool onlyUnknowns = false;
            bool extractFiles = true;
            string filterPattern = null;
            bool overwriteFiles = false;
            bool validateChecksums = false;
            bool verbose = false;

            var options = new OptionSet()
            {
                { "o|overwrite", "overwrite existing files", v => overwriteFiles = v != null },
                { "nf|no-files", "don't extract files", v => extractFiles = v == null },
                { "nu|no-unknowns", "don't extract unknown files", v => extractUnknowns = v == null },
                { "ou|only-unknowns", "only extract unknown files", v => onlyUnknowns = v != null },
                { "f|filter=", "only extract files using pattern", v => filterPattern = v },
                { "c|validate-checksums", "validate checksums of extracted files", v => validateChecksums = v != null },
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

            if (extras.Count < 1 || extras.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_a [output_dir]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Unpack files from a packed archive.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string inputPath = extras[0];
            string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(inputPath, null) + "_unpack";

            Regex filter = null;
            if (string.IsNullOrEmpty(filterPattern) == false)
            {
                filter = new Regex(filterPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            if (verbose == true)
            {
                Console.WriteLine("Loading project...");
            }

            var manager = ProjectData.Manager.Load();
            if (manager.ActiveProject == null)
            {
                Console.WriteLine("Warning: no active project loaded.");
            }

            using (var input = File.OpenRead(inputPath))
            {
                IArchiveFile archive;

                if (ArchiveFile.IsValid(input) == true)
                {
                    archive = new ArchiveFile();
                }
                else if (Antibirth.FileFormats.ArchiveFile.IsValid(input) == true)
                {
                    archive = new Antibirth.FileFormats.ArchiveFile();
                }
                else
                {
                    throw new NotSupportedException();
                }

                archive.Deserialize(input);

                var hashes = manager.LoadListsFileNames();

                if (extractFiles == true)
                {
                    var sortedEntries = archive.Entries.OrderBy(e => e.Offset).ToArray();

                    if (sortedEntries.Length > 0)
                    {
                        if (verbose == true)
                        {
                            Console.WriteLine("Unpacking files...");
                        }

                        long current = 0;
                        long total = sortedEntries.Length;
                        var padding = total.ToString(CultureInfo.InvariantCulture).Length;

                        var duplicates = new Dictionary<ulong, int>();

                        foreach (var entry in sortedEntries)
                        {
                            current++;

                            string entryName;
                            if (hashes.Contains(entry.NameHash) == false)
                            {
                                if (extractUnknowns == false)
                                {
                                    continue;
                                }

                                entryName = entry.NameHash.ToString("X16");
                                entryName = Path.Combine("__UNKNOWN", entryName + ".unknown");
                            }
                            else
                            {
                                if (onlyUnknowns == true)
                                {
                                    continue;
                                }

                                entryName = hashes[entry.NameHash];
                                entryName = FilterEntryName(entryName);
                            }

                            if (duplicates.ContainsKey(entry.NameHash) == true)
                            {
                                var number = duplicates[entry.NameHash]++;
                                var e = Path.GetExtension(entryName);
                                var nn =
                                    Path.ChangeExtension(
                                        Path.ChangeExtension(entryName, null) + "__DUPLICATE_" +
                                        number.ToString(CultureInfo.InvariantCulture),
                                        e);
                                entryName = Path.Combine("__DUPLICATE", nn);
                            }
                            else
                            {
                                duplicates[entry.NameHash] = 0;
                            }

                            if (filter != null &&
                                filter.IsMatch(entryName) == false)
                            {
                                continue;
                            }

                            var entryPath = Path.Combine(outputPath, entryName);
                            if (overwriteFiles == false &&
                                File.Exists(entryPath) == true)
                            {
                                continue;
                            }

                            if (verbose == true)
                            {
                                Console.WriteLine(
                                    "[{0}/{1}] {2}",
                                    current.ToString(CultureInfo.InvariantCulture).PadLeft(padding),
                                    total,
                                    entryName);
                            }

                            var entryParent = Path.GetDirectoryName(entryPath);
                            if (string.IsNullOrEmpty(entryParent) == false)
                            {
                                Directory.CreateDirectory(entryParent);
                            }

                            input.Seek(entry.Offset, SeekOrigin.Begin);
                            using (var data = entry.Read(input, archive))
                            {
                                if (archive.HasChecksums == true && validateChecksums == true)
                                {
                                    var bytes = data.ToArray();
                                    var checksum = ArchiveFile.ComputeChecksum(bytes, 0, bytes.Length);
                                    if (checksum != entry.Checksum)
                                    {
                                        Console.WriteLine(
                                            "checksum mismatch for {0}: {1:X} vs {2:X}",
                                            entryName,
                                            entry.Checksum,
                                            checksum);
                                    }
                                }

                                using (var output = File.Create(entryPath))
                                {
                                    output.WriteFromStream(data, entry.Length);
                                }
                            }
                        }
                    }
                }
            }
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
