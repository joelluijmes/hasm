using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using hasm.Parsing.Grammars;
using OfficeOpenXml;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace hasm.Parsing.Models
{
    public sealed class MicroInstruction
    {
        private static readonly Dictionary<string, Condition> _conditions = new Dictionary<string, Condition>
        {
            ["C"] = Condition.Carry,
            ["V"] = Condition.Overflow,
            ["Z"] = Condition.Zero,
            ["S"] = Condition.Sign,
            ["N"] = Condition.Negative
        };

        public string Label { get; set; }
        public ALU ALU { get; set; }
        public MemoryOperation Memory { get; set; }
        public bool LastInstruction { get; set; }
        public bool StatusEnabled { get; set; }
        public Condition Condition { get; set; }
        public bool InvertedCondition { get; set; }

        public MicroInstruction(string label, ALU alu, MemoryOperation memory, bool lastInstruction, bool statusEnabled, Condition condition, bool invertedCondition)
        {
            Label = label;
            ALU = alu;
            Memory = memory;
            LastInstruction = lastInstruction;
            StatusEnabled = statusEnabled;
            Condition = condition;
            InvertedCondition = invertedCondition;
        }

        public static MicroInstruction Parse(string[] row)
        {
            var instruction = row[0];
            var label = string.IsNullOrEmpty(instruction)
                ? string.Empty 
                : HasmGrammar.Opcode.FirstValue(instruction).ToLower() + "1";

            var operation = new Regex("\\s+").Replace(row[1], "");
            var parsed = MicroHasmGrammar.Operation.ParseTree(operation);

            var condition = Condition.None;
            var inverted = false;
            if (parsed.FirstNodeByNameOrDefault("if") != null)
            {
                var status = parsed.FirstValueByNameOrDefault<string>("status");
 
                if (_conditions.TryGetValue(status, out condition))
                     inverted = parsed.FirstValueByNameOrDefault<string>("cond") == "0";
            }

            var aluNode = parsed.FirstNodeByNameOrDefault("alu");
            var alu = aluNode != null
                ? ALU.Parse(aluNode)
                : null;

            var memoryCell = row[2];
            var memory = string.IsNullOrEmpty(memoryCell)
                ? MemoryOperation.None 
                : Grammar.EnumValue<MemoryOperation>().FirstValueOrDefault(memoryCell);

            var gotoInstruction = row[3];
            var lastInstruction = gotoInstruction == "next";

            var statusCell = row[4];
            var statusEnabled = statusCell == "1";
            
            return new MicroInstruction(label, alu, memory, lastInstruction, statusEnabled, condition, inverted);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append($"{Label}: ");
            if (Condition != Condition.None)
                builder.Append($"if {Condition} = {(InvertedCondition ? "0" : "1")}: ");

            builder.Append($"{ALU};");

            if (Memory != MemoryOperation.None)
                builder.Append($"{Memory};");

            if (LastInstruction)
                builder.Append(" next;");
            if (StatusEnabled)
                builder.Append(" status");

            return builder.ToString();
        }
    }
}