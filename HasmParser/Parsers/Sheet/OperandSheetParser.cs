using System.Collections.Generic;
using System.Linq;
using hasm.Parsing.Models;
using NLog;

namespace hasm.Parsing.Parsers.Sheet
{
    public sealed class OperandSheetParser : BaseSheetParser<OperandEncoding>
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IList<OperandEncoding> _items;

        protected override string SheetName => "Operands";

        public override IList<OperandEncoding> Items => _items ?? (_items = MergeOperands(ParseSheet()));

        protected override OperandEncoding Parse(string[] row, OperandEncoding previous)
        {
            var parser = OperandEncoding.Parse(row);
            if (parser.Operands == null)
            {
                parser.Operands = previous.Operands;
                parser.EncodingMask = previous.EncodingMask;
                parser.Size = previous.Size;
            }

            return parser;
        }

        private static IList<OperandEncoding> MergeOperands(IEnumerable<OperandEncoding> operands)
        {
            var merged = new List<OperandEncoding>();

            foreach (var group in operands.GroupBy(o => o.Operands))
            {
                var operand = group.First();
                if (operand.KeyValue != null)
                    operand.KeyValue = group.SelectMany(o => o.KeyValue).ToList();

                merged.Add(operand);
            }

            return merged;
        }
    }
}