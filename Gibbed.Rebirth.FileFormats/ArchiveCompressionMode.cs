using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.Rebirth.FileFormats
{
    public enum ArchiveCompressionMode : byte
    {
        Bogocrypt1 = 0,
        LZW = 1,
        MiniZ = 2,
        Bogocrypt2 = 5,
    }
}
