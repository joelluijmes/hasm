using System.Collections.Generic;
using System.Text.RegularExpressions;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace hasm.Parsing.Providers.SheetParser
{
    public sealed class MicroFunctionSheetProvider : BaseSheetProvider<MicroFunction>
    {
        private const int SHEET_INSTRUCTION = 0;
        private const int SHEET_ALU = 1;
        private const int SHEET_MEMORY = 2;
        private const int SHEET_GOTO = 3;
        private const int SHEET_STATUS = 4;
        private const int SHEET_BREAK = 5;
        
        protected override string SheetName => "MicroInstructions";

        protected override MicroFunction Parse(string[] row, MicroFunction previous)
        {
            var instruction = CreateMicroInstruction(row);
            if (!string.IsNullOrEmpty(row[SHEET_INSTRUCTION]))
                return new MicroFunction(row[SHEET_INSTRUCTION], instruction);

            previous.MicroInstructions.Add(instruction);
            return previous;
        }

        private static MicroInstruction CreateMicroInstruction(IList<string> row)
        {
            var operation = new Regex("\\s+").Replace(row[SHEET_ALU], "");
            var parsed = MicroHasmGrammar.Operation.ParseTree(operation);

            var alu = parsed.FirstValueOrDefault<Operation>() ?? Operation.NOP;

            var memoryCell = row[SHEET_MEMORY];
            var memory = string.IsNullOrEmpty(memoryCell)
                ? MemoryOperation.None
                : Grammar.EnumValue<MemoryOperation>().FirstValueOrDefault(memoryCell);

            var gotoInstruction = row[SHEET_GOTO];
            var lastInstruction = gotoInstruction == "next";
            
            var statusCell = row[SHEET_STATUS];
            var statusEnabled = statusCell == "1";

            var breakEnabled = row[SHEET_BREAK] == "1";

            return new MicroInstruction(alu, memory, lastInstruction, statusEnabled, breakEnabled);
        }
    }
}
