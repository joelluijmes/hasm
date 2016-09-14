using System;

namespace MicParser.OpCode
{
    [Flags]
    public enum ALU : long
    {
        SLL8 = 1 << 0,
        SRA1 = 1 << 1,
        Clear = 0,
        InverseSub = 1,
        Add = 3,
        Sub = 2,
        Or = 5,
        And = 6,
        Xor = 4,
        Preset = 7,
        H = 0 << 4,
        One = 1 << 4,
        Null = 2 << 4
    }
}