using System;

namespace MicParser.OpCode
{
    [Flags]
    public enum OutputRegister : long
    {
        H = 1 << 0,
        OPC = 1 << 1,
        TOS = 1 << 2,
        CPP = 1 << 3,
        LV = 1 << 4, 
        SP = 1 << 5, 
        PC = 1 << 6, 
        MDR = 1 << 7,
        MAR = 1 << 8,
    }
}