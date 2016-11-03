using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using hasm.Parsing.Grammars;
using NLog;
using ParserLib.Evaluation;

namespace hasm.Parsing.Models
{
    public sealed class ALU
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<string, AluOperation> _operations = new Dictionary<string, AluOperation>
        {
            ["-"] = AluOperation.Minus,
            ["+"] = AluOperation.Plus,
            ["&"] = AluOperation.And,
            ["|"] = AluOperation.Or,
            ["^"] = AluOperation.Xor
        };

        public string Target { get; set; }
        public string Left { get; set; }
        public string Right { get; set; }
        public bool Carry { get; set; }
        public bool StackPointer { get; set; }
        public string Shift { get; set; }
        public AluOperation Operation { get; set; }

        public static ALU Parse(string input)
        {
            input = new Regex("\\s+").Replace(input, "");
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));

            var alu = new ALU();
            var parsed = MicroHasmGrammar.Alu.ParseTree(input);

            alu.Target = parsed.FirstValueByNameOrDefault<string>("target");
            alu.Left = parsed.FirstValueByNameOrDefault<string>("left");
            var op = parsed.FirstValueByNameOrDefault<string>("op");
            if (op != null)
                alu.Operation = _operations[op];

            alu.Right = parsed.FirstValueByNameOrDefault<string>("right");
            alu.Carry = parsed.FirstValueByNameOrDefault<string>("carry") != null;
            alu.StackPointer = parsed.FirstValueByNameOrDefault<string>("SP") != null;
            alu.Shift = parsed.FirstValueByNameOrDefault<string>("shift");
            
            return null;
        }
    }

    public enum AluOperation
    {
        Clear = 0,
        Minus = 1 << 1,
        Plus = (1 << 1) | (1 << 0),
        Xor = 1 << 2,
        Or = (1 << 2) | (1 << 0),
        And = (1 << 2) | (1 << 1),
        Preset = (1 << 2) | (1 << 1) | (1 << 0)
    }
}