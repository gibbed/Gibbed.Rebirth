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
using System.Text;
using System.Collections.Generic;
using Gibbed.IO;

namespace Gibbed.Rebirth.FileFormats
{
    public class StageBinaryFile
    {
        private readonly List<Room> _Rooms;

        public List<Room> Rooms
        {
            get { return this._Rooms; }
        }

        public StageBinaryFile()
        {
            this._Rooms = new List<Room>();
        }

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            const Endian endian = Endian.Little;

            var roomCount = input.ReadValueU32(endian);

            var rooms = new Room[roomCount];
            for (uint i = 0; i < roomCount; i++)
            {
                var room = new Room();
                room.Type = input.ReadValueU32(endian);
                room.Variant = input.ReadValueU32(endian);
                room.Difficulty = input.ReadValueU8();
                room.Name = ReadString(input, endian);
                room.Weight = input.ReadValueF32(endian);
                room.Width = input.ReadValueU8();
                room.Height = input.ReadValueU8();

                var doorCount = input.ReadValueU8();
                var spawnCount = input.ReadValueU16(endian);

                room.Doors = new Door[doorCount];
                for (int j = 0; j < doorCount; j++)
                {
                    var door = new Door();
                    door.X = input.ReadValueS16(endian);
                    door.Y = input.ReadValueS16(endian);
                    door.Exists = input.ReadValueB8();
                    room.Doors[j] = door;
                }

                room.Spawns = new Spawn[spawnCount];
                for (int j = 0; j < spawnCount; j++)
                {
                    var spawn = new Spawn();
                    spawn.X = input.ReadValueS16(endian);
                    spawn.Y = input.ReadValueS16(endian);

                    var entityCount = input.ReadValueU8();

                    spawn.Entities = new Entity[entityCount];
                    for (int k = 0; k < entityCount; k++)
                    {
                        var entity = new Entity();
                        entity.Type = input.ReadValueU16(endian);
                        entity.Variant = input.ReadValueU16(endian);
                        entity.Subtype = input.ReadValueU16(endian);
                        entity.Weight = input.ReadValueF32(endian);
                        spawn.Entities[k] = entity;
                    }

                    room.Spawns[j] = spawn;
                }

                rooms[i] = room;
            }

            this._Rooms.Clear();
            this._Rooms.AddRange(rooms);
        }

        private static string ReadString(Stream input, Endian endian)
        {
            var length = input.ReadValueU16(endian);
            var text = input.ReadString(length, true, Encoding.ASCII);
            return text;
        }

        public struct Room
        {
            public uint Type;
            public uint Variant;
            public byte Difficulty;
            public string Name;
            public float Weight;
            public byte Width;
            public byte Height;
            public Door[] Doors;
            public Spawn[] Spawns;
        }

        public struct Door
        {
            public short X;
            public short Y;
            public bool Exists;
        }

        public struct Spawn
        {
            public short X;
            public short Y;
            public Entity[] Entities;
        }

        public struct Entity
        {
            public ushort Type;
            public ushort Variant;
            public ushort Subtype;
            public float Weight;
        }
    }
}
