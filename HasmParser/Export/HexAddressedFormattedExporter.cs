using System.IO;

namespace hasm.Parsing.Export
{
    public sealed class HexAddressedFormattedExporter : FormattedExporter
    {
        public HexAddressedFormattedExporter(Stream stream) : base(stream) {}

        protected override string FormatAddress(int address)
        {
            return $"{address:X4}h - {base.FormatAddress(address)}b";
        }
    }
}