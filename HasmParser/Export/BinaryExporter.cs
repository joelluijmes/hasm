using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace hasm.Parsing.Export
{
    public sealed class BinaryExporter : BaseExporter
    {
        public BinaryExporter(Stream stream) : base(stream) {}

        protected override async Task Export(IAssembled assembled)
        {
            if (assembled == null)
                return;

            var str = assembled.Bytes.Select(x => x.ToString("X2")).Aggregate((a, b) => $"{a} {b}");
            await Writer.WriteLineAsync(str);
        }
    }
}