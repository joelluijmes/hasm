using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hasm.Parsing.Export
{
    public sealed class IntelHexExporter : BaseExporter
    {
        protected override async Task Export(IAssembled assembled)
        {
            var hexRecord = assembled == null
                ? HexRecord.EOF
                : HexRecord.FromAssembled(assembled);

            await Writer.WriteLineAsync(hexRecord.ToString());
        }

        private sealed class HexRecord
        {
            public byte Code => (byte)':';
            public byte Length => (byte) (Data?.Length ?? 0);
            public short Address { get; set; }      
            public byte Type { get; set; }          
            public byte[] Data { get; set; }        
            public byte Checksum { get; set; }

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

            public static HexRecord FromAssembled(IAssembled assembled)
            {
                var address = (short)assembled.Address;
                var data = BitConverter.GetBytes(assembled.Assembled);

                var rounded = (int)Math.Ceiling(assembled.Count / 8.0);
                Array.Resize(ref data, rounded);
                Array.Reverse(data);

                return new HexRecord(address, data);
            }

            public static readonly HexRecord EOF;

            static HexRecord()
            {
                EOF = new HexRecord(0, null)
                {
                    Checksum = 0xFF,
                    Type = 0x01
                };
            }
        }

        public IntelHexExporter(Stream stream) : base(stream) {}
    }
}