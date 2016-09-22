using System;

namespace MicParser.OpCode
{
    [Flags]
    public enum ALU : long
    {
        SLL8 = 1L << 0 << 12,
        SRA1 = 1L << 1 << 12,
        Clear = 0L << 14,
        InverseSub = 1L << 14,
        Add = 3L << 14,
        Sub = 2L << 14,
        Or = 5L << 14,
        And = 6L << 14,
        Xor = 4L << 14,
        Preset = 7L << 14
    }
}