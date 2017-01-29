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

        public ReverseEndianAssembled(IAssembled original)
        {
            Address = original.Address;

            var buf = original.Bytes.ToArray();
            Array.Reverse(buf);
            Bytes = buf;
        }
    }
}
