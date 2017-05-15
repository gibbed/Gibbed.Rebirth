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

namespace Gibbed.Antibirth.FileFormats
{
    internal static class ArchiveConstants
    {
        public const ulong FirstEntryOffsetXor = 0x9FA57C6D7AF21DA5UL;
        public const ulong ParentEntryOffsetXor = 0x0194DB6267BEC200UL;
        public const ulong NextEntryOffsetXor = 0x0E3BB675A5B4D200UL;
        public const uint EntryDataSizeXor = 0x64E79669U;
        public const uint EntryDataMagicXor = 0xF885F00DU;
        public const ulong FirstChunkOffsetXor = 0xC1738B4E8C2449B1UL;
        public const ulong NextChunkOffsetXor = 0x02BA851EC27BF710UL;
    }
}
