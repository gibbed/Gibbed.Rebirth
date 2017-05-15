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

namespace Gibbed.Rebirth.FileFormats
{
    // ReSharper disable InconsistentNaming
    public sealed class ISAAC
        // ReSharper restore InconsistentNaming
    {
        /// <summary>
        /// log of size of rsl[] and mem[]
        /// </summary>
        public const int LogSize = 8;

        /// <summary>
        /// size of rsl[] and mem[]
        /// </summary>
        public const int Size = 1 << LogSize;

        /// <summary>
        /// for pseudorandom lookup
        /// </summary>
        public const int Mask = (Size - 1) << 2;

        private readonly int[] _Results;
        private readonly int[] _State;
        private int _Index;

        /// <summary>
        /// accumulator
        /// </summary>
        private int a;

        /// <summary>
        /// the last result
        /// </summary>
        private int b;

        /// <summary>
        /// counter, guarantees cycle is at least 2^^40
        /// </summary>
        private int c;

        /// <summary>
        /// no seed, equivalent to randinit(ctx,FALSE) in C
        /// </summary>
        public ISAAC()
            : this(null)
        {
        }

        /// <summary>
        /// equivalent to randinit(ctx, TRUE) after putting seed in randctx in C
        /// </summary>
        /// <param name="seed"></param>
        public ISAAC(int[] seed)
        {
            this._State = new int[Size];
            this._Results = new int[Size];

            if (seed == null)
            {
                this.Initialize(false);
            }
            else
            {
                Array.Copy(seed, 0, this._Results, 0, seed.Length);
                this.Initialize(true);
            }
        }

        /// <summary>
        /// Generate 256 results.  This is a fast (not small) implementation.
        /// </summary>
        private void Generate()
        {
            int i, j, x, y;

            b += ++c;
            for (i = 0, j = Size / 2; i < Size / 2;)
            {
                x = this._State[i];
                a ^= a << 13;
                a += this._State[j++];
                this._State[i] = y = this._State[(x & Mask) >> 2] + a + b;
                _Results[i++] = b = this._State[((y >> LogSize) & Mask) >> 2] + x;

                x = this._State[i];
                a ^= (int)((uint)a >> 6);
                a += this._State[j++];
                this._State[i] = y = this._State[(x & Mask) >> 2] + a + b;
                _Results[i++] = b = this._State[((y >> LogSize) & Mask) >> 2] + x;

                x = this._State[i];
                a ^= a << 2;
                a += this._State[j++];
                this._State[i] = y = this._State[(x & Mask) >> 2] + a + b;
                _Results[i++] = b = this._State[((y >> LogSize) & Mask) >> 2] + x;

                x = this._State[i];
                a ^= (int)((uint)a >> 16);
                a += this._State[j++];
                this._State[i] = y = this._State[(x & Mask) >> 2] + a + b;
                _Results[i++] = b = this._State[((y >> LogSize) & Mask) >> 2] + x;
            }

            for (j = 0; j < Size / 2;)
            {
                x = this._State[i];
                a ^= a << 13;
                a += this._State[j++];
                this._State[i] = y = this._State[(x & Mask) >> 2] + a + b;
                _Results[i++] = b = this._State[((y >> LogSize) & Mask) >> 2] + x;

                x = this._State[i];
                a ^= (int)((uint)a >> 6);
                a += this._State[j++];
                this._State[i] = y = this._State[(x & Mask) >> 2] + a + b;
                _Results[i++] = b = this._State[((y >> LogSize) & Mask) >> 2] + x;

                x = this._State[i];
                a ^= a << 2;
                a += this._State[j++];
                this._State[i] = y = this._State[(x & Mask) >> 2] + a + b;
                _Results[i++] = b = this._State[((y >> LogSize) & Mask) >> 2] + x;

                x = this._State[i];
                a ^= (int)((uint)a >> 16);
                a += this._State[j++];
                this._State[i] = y = this._State[(x & Mask) >> 2] + a + b;
                _Results[i++] = b = this._State[((y >> LogSize) & Mask) >> 2] + x;
            }
        }

        /// <summary>
        /// initialize, or reinitialize, this instance of rand
        /// </summary>
        /// <param name="flag"></param>
        private void Initialize(bool flag)
        {
            int i;
            int a, b, c, d, e, f, g, h;
            a = b = c = d = e = f = g = h = unchecked((int)0x9E3779B9); /* the golden ratio */

            for (i = 0; i < 4; ++i)
            {
                a ^= b << 11;
                d += a;
                b += c;
                b ^= (int)((uint)c >> 2);
                e += b;
                c += d;
                c ^= d << 8;
                f += c;
                d += e;
                d ^= (int)((uint)e >> 16);
                g += d;
                e += f;
                e ^= f << 10;
                h += e;
                f += g;
                f ^= (int)((uint)g >> 4);
                a += f;
                g += h;
                g ^= h << 8;
                b += g;
                h += a;
                h ^= (int)((uint)a >> 9);
                c += h;
                a += b;
            }

            for (i = 0; i < Size; i += 8)
            {
                /* fill in mem[] with messy stuff */
                if (flag)
                {
                    a += _Results[i + 0];
                    b += _Results[i + 1];
                    c += _Results[i + 2];
                    d += _Results[i + 3];
                    e += _Results[i + 4];
                    f += _Results[i + 5];
                    g += _Results[i + 6];
                    h += _Results[i + 7];
                }
                a ^= b << 11;
                d += a;
                b += c;
                b ^= (int)((uint)c >> 2);
                e += b;
                c += d;
                c ^= d << 8;
                f += c;
                d += e;
                d ^= (int)((uint)e >> 16);
                g += d;
                e += f;
                e ^= f << 10;
                h += e;
                f += g;
                f ^= (int)((uint)g >> 4);
                a += f;
                g += h;
                g ^= h << 8;
                b += g;
                h += a;
                h ^= (int)((uint)a >> 9);
                c += h;
                a += b;
                this._State[i] = a;
                this._State[i + 1] = b;
                this._State[i + 2] = c;
                this._State[i + 3] = d;
                this._State[i + 4] = e;
                this._State[i + 5] = f;
                this._State[i + 6] = g;
                this._State[i + 7] = h;
            }

            if (flag)
            {
                /* second pass makes all of seed affect all of mem */
                for (i = 0; i < Size; i += 8)
                {
                    a += this._State[i + 0];
                    b += this._State[i + 1];
                    c += this._State[i + 2];
                    d += this._State[i + 3];
                    e += this._State[i + 4];
                    f += this._State[i + 5];
                    g += this._State[i + 6];
                    h += this._State[i + 7];
                    a ^= b << 11;
                    d += a;
                    b += c;
                    b ^= (int)((uint)c >> 2);
                    e += b;
                    c += d;
                    c ^= d << 8;
                    f += c;
                    d += e;
                    d ^= (int)((uint)e >> 16);
                    g += d;
                    e += f;
                    e ^= f << 10;
                    h += e;
                    f += g;
                    f ^= (int)((uint)g >> 4);
                    a += f;
                    g += h;
                    g ^= h << 8;
                    b += g;
                    h += a;
                    h ^= (int)((uint)a >> 9);
                    c += h;
                    a += b;
                    this._State[i + 0] = a;
                    this._State[i + 1] = b;
                    this._State[i + 2] = c;
                    this._State[i + 3] = d;
                    this._State[i + 4] = e;
                    this._State[i + 5] = f;
                    this._State[i + 6] = g;
                    this._State[i + 7] = h;
                }
            }

            this.Generate();
        }

        /// <summary>
        /// Call rand.val() to get a random value
        /// </summary>
        /// <returns></returns>
        public int Value()
        {
            var index = this._Index;
            var result = _Results[index];
            index++;

            if (index >= Size)
            {
                this.Generate();
                this._Index = 0;
            }
            else
            {
                this._Index = index;
            }
            return result;
        }
    }
}
