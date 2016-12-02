using System;
using System.Collections.Generic;
using System.Linq;
using hasm.Parsing.Models;
using NLog;

namespace hasm.Parsing.Providers.SheetParser
{
    public sealed class OperandSheetProvider : BaseSheetProvider<OperandEncoding>
    {
        private const int SHEET_OPERAND = 0;
        private const int SHEET_MASK = 1;
        private const int SHEET_BITS = 2;
        private const int SHEET_BUS = 3;
        private const int SHEET_KEY = 4;
        private const int SHEET_VALUE = 5;
        private const int SHEET_MIN = 6;
        private const int SHEET_MAX = 7;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IList<OperandEncoding> _items;

        protected override string SheetName => "Operands";

        public override IList<OperandEncoding> Items => _items ?? (_items = MergeOperands(ParseSheet()));

        protected override OperandEncoding Parse(string[] row, OperandEncoding previous)
        {
            var operand = !string.IsNullOrEmpty(row[SHEET_OPERAND])
                ? row[SHEET_OPERAND].Split(',')
                : null;

            OperandEncoding operandEncoding;

            var mask = !string.IsNullOrEmpty(row[SHEET_MASK])
                ? row[SHEET_MASK][0]
                : '\0';
            var bits = !string.IsNullOrEmpty(row[SHEET_BITS])
                ? int.Parse(row[SHEET_BITS])
                : 0;
            if (!string.IsNullOrEmpty(row[SHEET_KEY]) && !string.IsNullOrEmpty(row[SHEET_VALUE]) && string.IsNullOrEmpty(row[SHEET_MIN]) && string.IsNullOrEmpty(row[SHEET_MAX]))
            { // keyvalue
                var keyValue = new KeyValuePair<string, int>(row[SHEET_KEY], int.Parse(row[SHEET_VALUE]));
                operandEncoding = new OperandEncoding(operand, mask, bits, keyValue);
            }
            else
            {
                if (string.IsNullOrEmpty(row[SHEET_KEY]) && string.IsNullOrEmpty(row[SHEET_VALUE]) && !string.IsNullOrEmpty(row[SHEET_MIN]) && !string.IsNullOrEmpty(row[SHEET_MAX]))
                { // range
                    var min = int.Parse(row[SHEET_MIN]);
                    var max = int.Parse(row[SHEET_MAX]);
                    operandEncoding = new OperandEncoding(operand, mask, bits, min, max);
                }
                else
                {
                    if (string.IsNullOrEmpty(row[SHEET_MASK]) && string.IsNullOrEmpty(row[SHEET_BITS]) && string.IsNullOrEmpty(row[SHEET_KEY]) && string.IsNullOrEmpty(row[SHEET_VALUE]) && string.IsNullOrEmpty(row[SHEET_MIN]) && string.IsNullOrEmpty(row[SHEET_MAX]))
                    { // aggregation
                        operandEncoding = new OperandEncoding(operand);
                    }
                    else
                        throw new NotImplementedException();
                }
            }

            if (operandEncoding.Operands == null)
            {
                operandEncoding.Operands = previous.Operands;
                operandEncoding.EncodingMask = previous.EncodingMask;
                operandEncoding.Size = previous.Size;
            }

            return operandEncoding;
        }

        private static IList<OperandEncoding> MergeOperands(IEnumerable<OperandEncoding> operands)
        {
            var merged = new List<OperandEncoding>();

            foreach (var group in operands.GroupBy(o => o.Operands))
            {
                var operand = group.First();
                if (operand.Pairs != null)
                    operand.Pairs = group.SelectMany(o => o.Pairs).ToList();

                merged.Add(operand);
            }

            return merged;
        }
    }
}
