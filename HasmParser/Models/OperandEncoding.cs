using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hasm.Parsing.Models
{
    public sealed class OperandEncoding
    {
        public OperandEncoding(string[] operands)
        {
            Operands = operands;
            Type = OperandEncodingType.Aggregation;
        }

        public OperandEncoding(string[] operands, char encodingMask, int size, KeyValuePair<string, int> keyValue)
        {
            Operands = operands;
            EncodingMask = encodingMask;
            Size = size;
            Pairs = new[] {keyValue};
            Type = OperandEncodingType.KeyValue;
        }

        public OperandEncoding(string[] operands, char encodingMask, int size, int minimum, int maximum)
        {
            Operands = operands;
            EncodingMask = encodingMask;
            Size = size;
            Minimum = minimum;
            Maximum = maximum;
            Type = OperandEncodingType.Range;
        }

        public string[] Operands { get; set; }
        public char EncodingMask { get; set; }
        public int Size { get; set; }
        public IList<KeyValuePair<string, int>> Pairs { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }
        public OperandEncodingType Type { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Operands.Aggregate((a, b) => $"{a},{b}"));
            builder.Append($" {Type}");

            return builder.ToString();
        }
    }
}
