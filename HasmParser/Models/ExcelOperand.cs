using System.Collections.Generic;

namespace hasm.Parsing.Models
{
    public sealed class ExcelOperand
    {
        public ExcelOperand(string[] operands, char encodingMask, int size, KeyValuePair<string, int> keyValue)
        {
            Operands = operands;
            EncodingMask = encodingMask;
            Size = size;
            KeyValue = new[] {keyValue};
        }

        public ExcelOperand(string[] operands, char encodingMask, int size, int minimum, int maximum)
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

        public static ExcelOperand Parse(string[] row)
        {
            var operand = !string.IsNullOrEmpty(row[0]) ? row[0].Split(',') : null;
            var mask = !string.IsNullOrEmpty(row[1]) ? row[1][0] : '\0';
            var bits = !string.IsNullOrEmpty(row[2]) ? int.Parse(row[2]) : 0;

            var keyValue = new KeyValuePair<string, int>(row[3], int.Parse(row[4]));

            return new ExcelOperand(operand, mask, bits, keyValue);
        }
    }
}