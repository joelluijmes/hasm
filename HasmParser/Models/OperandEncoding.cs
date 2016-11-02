using System.Collections.Generic;

namespace hasm.Parsing.Models
{
    public sealed class OperandEncoding
    {
        public OperandEncoding(string[] operands, char encodingMask, int size, KeyValuePair<string, int> keyValue)
        {
            Operands = operands;
            EncodingMask = encodingMask;
            Size = size;
            KeyValue = new[] {keyValue};
        }

        public OperandEncoding(string[] operands, char encodingMask, int size, int minimum, int maximum)
        {
            Operands = operands;
            EncodingMask = encodingMask;
            Size = size;
            Minimum = minimum;
            Maximum = maximum;
        }

        public string[] Operands { get; set; }
        public char EncodingMask { get; set; }
        public int Size { get; set; }
        public IList<KeyValuePair<string, int>> KeyValue { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public static OperandEncoding Parse(string[] row)
        {
            var operand = !string.IsNullOrEmpty(row[0]) ? row[0].Split(',') : null;
            var mask = !string.IsNullOrEmpty(row[1]) ? row[1][0] : '\0';
            var bits = !string.IsNullOrEmpty(row[2]) ? int.Parse(row[2]) : 0;

            if (!string.IsNullOrEmpty(row[3]) && !string.IsNullOrEmpty(row[4]))
            {
                var keyValue = new KeyValuePair<string, int>(row[3], int.Parse(row[4]));
                return new OperandEncoding(operand, mask, bits, keyValue);
            }

            var min = int.Parse(row[5]);
            var max = int.Parse(row[6]);
            return new OperandEncoding(operand, mask, bits, min, max);
        }
    }
}