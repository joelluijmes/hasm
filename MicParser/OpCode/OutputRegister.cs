using System;

namespace MicParser.OpCode
{
    [Flags]
    public enum OutputRegister : long
    {
        H = 1L << 0 << 20,
        OPC = 1L << 1 << 20,
        TOS = 1L << 2 << 20,
        CPP = 1L << 3 << 20,
        LV = 1L << 4 << 20,
        SP = 1L << 5 << 20,
        PC = 1L << 6 << 20,
        MDR = 1L << 7 << 20,
        MAR = 1L << 8 << 20
    }
}