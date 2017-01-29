using System;
using System.Collections.Generic;
using System.Linq;

namespace hasm.Parsing.Export
{
    public interface IAssembled
    {
        int Address { get; set; }
        byte[] Bytes { get; }
    }

    public class RawAssembled : IAssembled
    {
        public int Address { get; set; }
        public byte[] Bytes { get; }

        public RawAssembled(int address, byte[] bytes)
        {
            Address = address;
            Bytes = bytes;
        }
    }

    public class ReverseEndianAssembled : IAssembled
    {
        public int Address { get; set; }
        public byte[] Bytes { get; }

        private string _toString;

        public ReverseEndianAssembled(int address, byte[] bytes)
        {
            Address = address;

            var buf = bytes.ToArray();
            Array.Reverse(buf);
            Bytes = buf;
        }

        public static IAssembled Create(IAssembled original)
        {
            var assembled = new ReverseEndianAssembled(original.Address, original.Bytes) {_toString = original.ToString()};

            return assembled;
        }

        public override string ToString() => _toString;
    }
}
