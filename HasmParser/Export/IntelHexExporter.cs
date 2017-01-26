﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hasm.Parsing.Export
{
    public sealed class IntelHexExporter : BaseExporter
    {
        public IntelHexExporter(Stream stream) : base(stream) {}

        protected override async Task Export(IAssembled assembled)
        {
            var hexRecord = assembled == null
                ? HexRecord.EOF
                : new HexRecord((short)assembled.Address, assembled.Bytes);

            await Writer.WriteLineAsync(hexRecord.ToString());
        }

        private sealed class HexRecord
        {
            public static readonly HexRecord EOF;

            static HexRecord()
            {
                EOF = new HexRecord(0, null)
                {
                    Checksum = 0xFF,
                    Type = 0x01
                };
            }

            public HexRecord(short address, byte[] data)
            {
                Address = address;
                Type = 0;
                Data = data;

                if (Data == null)
                    return;

                var computedChecksum = (byte) (Length + (byte) (Address >> 8) + (byte) Address);
                computedChecksum = Data.Aggregate(computedChecksum, (a, b) => (byte) (a + b));
                Checksum = (byte) -computedChecksum;
            }

            public byte Code => (byte) ':';
            public byte Length => (byte) (Data?.Length ?? 0);
            public short Address { get; }
            public byte Type { get; set; }
            public byte[] Data { get; }
            public byte Checksum { get; set; }

            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.Append((char) Code);
                builder.Append(Length.ToString("X2"));
                builder.Append(Address.ToString("X4"));
                builder.Append(Type.ToString("X2"));

                if (Data != null)
                {
                    foreach (var d in Data)
                        builder.Append(d.ToString("X2"));
                }

                builder.Append(Checksum.ToString("X2"));
                return builder.ToString();
            }
        }
    }
}
