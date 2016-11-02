using System;
using System.Collections.Generic;
using hasm.Parsing.Models;
using NLog;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace hasm.Parsing.Parsers
{
    public sealed class MicroHasmParser : BaseParser<MicroInstruction>
    {

        public MicroHasmParser()
        {
            Instructions = ParseSheet();
        }

        public IList<MicroInstruction> Instructions { get; }

        protected override string SheetName => "MicroInstructions";
        protected override MicroInstruction Parse(string[] row, MicroInstruction previous)
        {
            var countingLabelRule = Grammar.Text(Grammar.Letters) + Grammar.Int32();

            var instruction = MicroInstruction.Parse(row);
            if (string.IsNullOrEmpty(instruction.Label))
            {
                if (previous == null)
                    throw new NotImplementedException();

                var name = countingLabelRule.FirstValue<string>(previous.Label);
                var index = countingLabelRule.FirstValue<int>(previous.Label);

                instruction.Label = $"{name}{++index}";
            }

            return instruction;
        }
    }
}