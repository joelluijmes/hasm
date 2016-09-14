using System;

namespace MicParser.OpCode
{
    [Flags]
    public enum Memory : long
    {
        Write = 1 << 0,
        Read = 1 << 1,
        Fetch = 1 << 2
    }
}