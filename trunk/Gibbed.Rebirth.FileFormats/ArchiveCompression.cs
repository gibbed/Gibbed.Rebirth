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
using System.IO;
using Gibbed.IO;

namespace Gibbed.Rebirth.FileFormats
{
    public class ArchiveCompression
    {
        private class CodeDictionary
        {
            private struct Item
            {
                public int PreviousIndex;
                public byte Value;
            }

            private readonly List<Item> _Items;
            private int _CodeLength;
            private readonly object _DecodeWindowLock;
            private readonly byte[] _DecodeWindow;

            public const int Capacity = 4096;

            public int Count
            {
                get { return this._Items.Count; }
            }

            public int CodeLength
            {
                get { return this._CodeLength; }
            }

            public CodeDictionary()
            {
                this._Items = new List<Item>();
                this._DecodeWindowLock = new object();
                this._DecodeWindow = new byte[Capacity];
                this.Reset();
            }

            public void Reset()
            {
                var items = new List<Item>();
                for (int i = 0; i < 256; i++)
                {
                    Item record;
                    record.Value = (byte)i;
                    record.PreviousIndex = -1;
                    items.Add(record);
                }
                this._Items.Clear();
                this._Items.AddRange(items);
                this._CodeLength = 8;
            }

            public void Add(byte lastValue, int previousIndex)
            {
                if (this._Items.Count >= Capacity)
                {
                    throw new InvalidOperationException();
                }

                var previousItem = this._Items[previousIndex];

                Item item;
                item.Value = lastValue;
                item.PreviousIndex = previousIndex;

                this._Items[previousIndex] = previousItem;
                this._Items.Add(item);
            }

            public void UpdateCodeLength()
            {
                while (this._Items.Count >= 1 << this._CodeLength)
                {
                    this._CodeLength++;
                }
            }

            public byte[] Decode(int index, out byte lastValue)
            {
                byte[] bytes;
                lock (this._DecodeWindowLock)
                {
                    int decodeWindowLength = this._DecodeWindow.Length;
                    var writePosition = decodeWindowLength;

                    while (index != -1)
                    {
                        writePosition--; 
                        if (writePosition < 0)
                        {
                            throw new InvalidOperationException();
                        }

                        var item = this._Items[index];
                        this._DecodeWindow[writePosition] = item.Value;
                        index = item.PreviousIndex;
                    }

                    lastValue = this._DecodeWindow[writePosition];
                    bytes = new byte[decodeWindowLength - writePosition];
                    Array.Copy(this._DecodeWindow, writePosition, bytes, 0, bytes.Length);
                }
                return bytes;
            }
        }

        public static void Decompress(ArchiveFile.Entry entry, Stream input, Stream output, Endian endian)
        {
            var dictionary = new CodeDictionary();

            long remaining = entry.Length;
            while (remaining > 0)
            {
                var length = input.ReadValueU32(endian);
                var bytes = input.ReadBytes(length);

                var reader = new BitReader(bytes);
                
                byte[] line;
                byte lastValue;
                int previousIndex = reader.ReadInt32(dictionary.CodeLength);
                line = dictionary.Decode(previousIndex, out lastValue);
                output.WriteBytes(line);
                remaining -= line.Length;

                while (reader.Position < reader.Length)
                {
                    if (dictionary.Count + 1 >= CodeDictionary.Capacity)
                    {
                        dictionary.Reset();

                        previousIndex = reader.ReadInt32(dictionary.CodeLength);
                        line = dictionary.Decode(previousIndex, out lastValue);
                        output.WriteBytes(line);
                        remaining -= line.Length;
                        continue;
                    }

                    dictionary.UpdateCodeLength();
                    var index = reader.ReadInt32(dictionary.CodeLength);

                    if (index == dictionary.Count)
                    {
                        if (previousIndex != -1)
                        {
                            dictionary.Add(lastValue, previousIndex);
                        }

                        line = dictionary.Decode(index, out lastValue);
                        output.WriteBytes(line);
                        remaining -= line.Length;
                    }
                    else
                    {
                        if (index > dictionary.Count)
                        {
                            throw new InvalidOperationException();
                        }

                        line = dictionary.Decode(index, out lastValue);
                        output.WriteBytes(line);
                        remaining -= line.Length;

                        if (previousIndex != -1)
                        {
                            dictionary.Add(lastValue, previousIndex);
                        }
                    }

                    previousIndex = index;
                }

                if (reader.Position != reader.Length)
                {
                    throw new InvalidOperationException();
                }
            }

            if (remaining < 0)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
