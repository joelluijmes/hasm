using System;

namespace MicParser.OpCode
{
    [Flags]
    public enum JAM : long
    {
        JMPC = 1L << 0 << 9,
        JAMN = 1L << 1 << 9,
        JAMZ = 1L << 2 << 9
    }
}