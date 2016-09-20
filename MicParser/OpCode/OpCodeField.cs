using System;

namespace MicParser.OpCode
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class OpCodeField : Attribute
    {
        public OpCodeField(uint bits)
        {
            Bits = bits;
        }

        public uint Bits { get; set; }
    }
}