﻿namespace MicParser.InstructionTypes
{
    public enum AluOperation : long
    {
        InverseSub = 1 << 14,
        Add = 3 << 14,
        Sub = 2 << 14,
        Or = 5 << 14,
        And = 6 << 14,
        Xor = 4 << 14,
        Preset = 7 << 14,
        Clear = 0 << 14
    }
}