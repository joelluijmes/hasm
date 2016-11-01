using System.Collections.Generic;
using System.Linq;
using hasm.Parsing.Models;
using NLog;

namespace hasm.Parsing.Parsers
{
    public sealed class OperandParser : BaseParser<ExcelOperand>
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public IList<ExcelOperand> ExcelOperandParser { get; }

        public OperandParser()
        {
            var operands = ParseSheet();
            ExcelOperandParser = MergeOperands(operands);
        }

        protected override string SheetName => "Operands";

        protected override ExcelOperand Parse(string[] row, ExcelOperand previous)
        {
            var parser = ExcelOperand.Parse(row);
            if (parser.Operands == null)
                parser.Operands = previous.Operands;
            if (parser.EncodingMask == '\0')
                parser.EncodingMask = previous.EncodingMask;
            if (parser.Size == 0)
                parser.Size = previous.Size;

            return parser;
        }

        private static IList<ExcelOperand> MergeOperands(IEnumerable<ExcelOperand> operands)
        {
            var merged = new List<ExcelOperand>();

            foreach (var group in operands.GroupBy(o => o.Operands))
            {
                var operand = group.First();
                operand.KeyValue = group.SelectMany(o => o.KeyValue).ToList();

                merged.Add(operand);
            }

            return merged;
        }
    }
}