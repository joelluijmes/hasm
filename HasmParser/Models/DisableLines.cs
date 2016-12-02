using System;

namespace hasm.Parsing.Models
{
    [Flags]
    internal enum DisableLines
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Target = 1 << 2,
        Immediate = 1 << 3
    }
}
