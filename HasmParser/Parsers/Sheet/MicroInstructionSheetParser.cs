using System;
using hasm.Parsing.Models;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace hasm.Parsing.Parsers.Sheet
{
    public sealed class MicroInstructionSheetParser : BaseSheetParser<MicroInstruction>
    {
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