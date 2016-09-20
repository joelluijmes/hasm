using System;

namespace MicParser.OpCode
{
    [Flags]
    public enum Memory : long
    {
        Write = 1L << 0 << 29,
        Read = 1L << 1 << 29,
        Fetch = 1L << 2 << 29
    }
}