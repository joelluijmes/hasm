using System;

namespace MicParser.OpCode
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class OpCodeField : Attribute
    {
        public uint Bits { get; set; }

        public OpCodeField(uint bits)
        {
            Bits = bits;
        }
    }
}