using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hasm.Parsing.Export
{
    public class FormattedExporter : BaseExporter
    {
        private static readonly Regex _spaceRegex = new Regex(".{4}");

        public FormattedExporter(Stream stream) : base(stream) {}

        public bool AppendToString { get; set; } = false;
        public int Base { get; set; } = 16;

        protected virtual string FormatAddress(int address)
        {
            var padding = Base == 16
                ? 4
                : Base == 2
                    ? 16
                    : 0;

            var str = Convert.ToString(address, Base).ToUpper().PadLeft(padding, '0');
            return _spaceRegex.Replace(str, "$0 ").Trim();
        }

        protected virtual string FormatAssembled(byte[] bytes, int padding = 0)
        {
            if (padding == 0)
            {
                padding = Base == 16
                    ? 16
                    : Base == 2
                        ? 64
                        : 0;
            }

            Array.Resize(ref bytes, sizeof(long));
            var assembled = BitConverter.ToInt64(bytes, 0);
            var str = Convert.ToString(assembled, Base).ToUpper().PadLeft(padding, '0');
            return _spaceRegex.Replace(str, "$0 ").Trim();
        }

        protected override async Task Export(IAssembled assembled)
        {
            if (assembled == null)
                return;

            await Writer.WriteLineAsync($"{FormatAddress(assembled.Address)}: {FormatAssembled(assembled.Bytes)} {(AppendToString ? assembled.ToString() : "")}");
        }
    }
}
