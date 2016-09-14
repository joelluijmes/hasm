using System;

namespace MicParser.OpCode
{
    [Flags]
    public enum JAM : long
    {
        JMPC = 1 << 0,
        JAMN = 1 << 1,
        JAMZ = 1 << 2
    }
}