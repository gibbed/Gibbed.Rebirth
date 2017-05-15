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
using Gibbed.IO;

namespace Gibbed.Afterbirth.FileFormats
{
    public class StageBinaryFile
    {
        public const uint Signature = 0x31425453u; // 'STB1'

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
            const Endian endian = Endian.Little;

            output.WriteValueU32(Signature, endian);
            output.WriteValueS32(this._Rooms.Count, endian);
            foreach (var room in this._Rooms)
            {
                output.WriteValueU32(room.Type, endian);
                output.WriteValueU32(room.Variant, endian);
                output.WriteValueU32(room.Subtype, endian);
                output.WriteValueU8(room.Difficulty);
                output.WriteString(room.Name, endian);
                output.WriteValueF32(room.Weight, endian);
                output.WriteValueU8(room.Width);
                output.WriteValueU8(room.Height);
                output.WriteValueU8(room.Shape);

                var doorCount = room.Doors == null ? 0 : room.Doors.Length;
                if (doorCount > byte.MaxValue)
                {
                    throw new InvalidOperationException();
                }

                var spawnCount = room.Spawns == null ? 0 : room.Spawns.Length;
                if (spawnCount > ushort.MaxValue)
                {
                    throw new InvalidOperationException();
                }

                output.WriteValueU8((byte)doorCount);
                output.WriteValueU16((ushort)spawnCount, endian);

                if (room.Doors != null)
                {
                    foreach (var door in room.Doors)
                    {
                        output.WriteValueS16(door.X, endian);
                        output.WriteValueS16(door.Y, endian);
                        output.WriteValueB8(door.Exists);
                    }
                }

                if (room.Spawns != null)
                {
                    foreach (var spawn in room.Spawns)
                    {
                        output.WriteValueS16(spawn.X, endian);
                        output.WriteValueS16(spawn.Y, endian);

                        var entityCount = spawn.Entities == null ? 0 : spawn.Entities.Length;
                        if (entityCount > byte.MaxValue)
                        {
                            throw new InvalidOperationException();
                        }

                        output.WriteValueU8((byte)entityCount);

                        if (spawn.Entities != null)
                        {
                            foreach (var entity in spawn.Entities)
                            {
                                output.WriteValueU16(entity.Type, endian);
                                output.WriteValueU16(entity.Variant, endian);
                                output.WriteValueU16(entity.Subtype, endian);
                                output.WriteValueF32(entity.Weight, endian);
                            }
                        }
                    }
                }
            }
        }

        public void Deserialize(Stream input)
        {
            const Endian endian = Endian.Little;

            var magic = input.ReadValueU32(endian);
            if (magic != Signature)
            {
                throw new FormatException();
            }

            var roomCount = input.ReadValueU32(endian);

            var rooms = new Room[roomCount];
            for (uint i = 0; i < roomCount; i++)
            {
                var room = new Room();
                room.Type = input.ReadValueU32(endian);
                room.Variant = input.ReadValueU32(endian);
                room.Subtype = input.ReadValueU32(endian);
                room.Difficulty = input.ReadValueU8();
                room.Name = input.ReadString(endian);
                room.Weight = input.ReadValueF32(endian);
                room.Width = input.ReadValueU8();
                room.Height = input.ReadValueU8();
                room.Shape = input.ReadValueU8();

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

        public struct Room
        {
            public uint Type;
            public uint Variant;
            public uint Subtype;
            public byte Difficulty;
            public string Name;
            public float Weight;
            public byte Width;
            public byte Height;
            public byte Shape;
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
