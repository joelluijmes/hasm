using System;
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
            EncodingType = OperandEncodingType.Aggregation;
        }

        public OperandEncoding(string[] operands, char encodingMask, int size, KeyValuePair<string, int> keyValue)
        {
            Operands = operands;
            EncodingMask = encodingMask;
            Size = size;
            KeyValue = new[] {keyValue};
            EncodingType = OperandEncodingType.KeyValue;
        }

        public OperandEncoding(string[] operands, char encodingMask, int size, int minimum, int maximum)
        {
            Operands = operands;
            EncodingMask = encodingMask;
            Size = size;
            Minimum = minimum;
            Maximum = maximum;
            EncodingType = OperandEncodingType.Range;
        }

        public string[] Operands { get; set; }
        public char EncodingMask { get; set; }
        public int Size { get; set; }
        public IList<KeyValuePair<string, int>> KeyValue { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }
        public OperandEncodingType EncodingType { get; set; }

        public static OperandEncoding Parse(string[] row)
        {
            var operand = !string.IsNullOrEmpty(row[0]) ? row[0].Split(',') : null;

            var mask = !string.IsNullOrEmpty(row[1]) ? row[1][0] : '\0';
            var bits = !string.IsNullOrEmpty(row[2]) ? int.Parse(row[2]) : 0;
            if (!string.IsNullOrEmpty(row[3]) && !string.IsNullOrEmpty(row[4]) && string.IsNullOrEmpty(row[5]) && string.IsNullOrEmpty(row[6]))
            { // keyvalue
                var keyValue = new KeyValuePair<string, int>(row[3], int.Parse(row[4]));
                return new OperandEncoding(operand, mask, bits, keyValue);
            }

            if (string.IsNullOrEmpty(row[3]) && string.IsNullOrEmpty(row[4]) && !string.IsNullOrEmpty(row[5]) && !string.IsNullOrEmpty(row[6]))
            { // range
                var min = int.Parse(row[5]);
                var max = int.Parse(row[6]);
                return new OperandEncoding(operand, mask, bits, min, max);
            }

            if (string.IsNullOrEmpty(row[1]) && string.IsNullOrEmpty(row[2]) && string.IsNullOrEmpty(row[3]) && string.IsNullOrEmpty(row[4]) && string.IsNullOrEmpty(row[5]) && string.IsNullOrEmpty(row[6]))
            { // aggregation
                return new OperandEncoding(operand);    
            }

            throw new NotImplementedException();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Operands.Aggregate((a, b) => $"{a},{b}"));
            builder.Append($" {EncodingType}");

            return builder.ToString();
        }
    }
}