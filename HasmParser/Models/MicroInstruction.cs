using System;
using hasm.Parsing.Grammars;
using OfficeOpenXml;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace hasm.Parsing.Models
{
    public sealed class MicroInstruction
    {
        public string Label { get; set; }
        public ALU ALU { get; set; }
        public MemoryOperation Memory { get; set; }
        public bool LastInstruction { get; set; }
        public bool StatusEnabled { get; set; }


        public MicroInstruction(string label, ALU alu, MemoryOperation memory, bool lastInstruction, bool statusEnabled)
        {
            Label = label;
            ALU = alu;
            Memory = memory;
            LastInstruction = lastInstruction;
            StatusEnabled = statusEnabled;
        }

        public static MicroInstruction Parse(string[] row)
        {
            var instruction = row[0];
            var label = string.IsNullOrEmpty(instruction)
                ? string.Empty 
                : HasmGrammar.Opcode.FirstValue(instruction).ToLower() + "1";

            var aluCell = row[1];
            var status = parsed.FirstValueByNameOrDefault<string>("status");
            var cond = parsed.FirstValueByNameOrDefault<string>("cond");

            var alu = !string.IsNullOrEmpty(aluCell)
                ? ALU.Parse(aluCell)
                : null;

            var memoryCell = row[2];
            var memory = string.IsNullOrEmpty(memoryCell)
                ? MemoryOperation.None 
                : Grammar.EnumValue<MemoryOperation>().FirstValueOrDefault(memoryCell);

            var gotoInstruction = row[3];
            var lastInstruction = gotoInstruction == "next";

            var statusCell = row[4];
            var statusEnabled = statusCell == "1";
            
            return new MicroInstruction(label, alu, memory, lastInstruction, statusEnabled);
        }

        public override string ToString() => $"{Label}: {ALU};{(Memory != MemoryOperation.None ? $"{Memory};" : "")}{(LastInstruction ? "next" : "")}";
    }

    public enum MemoryOperation
    {
        None,
        Read,
        Write
    }
}