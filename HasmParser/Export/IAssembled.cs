using System;
using System.Collections.Generic;

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
}
