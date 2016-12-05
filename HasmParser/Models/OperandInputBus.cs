using System;

namespace hasm.Parsing.Models
{
    [Flags]
    public enum OperandInputBus
    {
        Unkown,
        Left,
        Right,
        Both = Left | Right
    }
}