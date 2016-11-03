using System;
using System.Text.RegularExpressions;
using hasm.Parsing.Grammars;
using NLog;
using ParserLib;
using ParserLib.Evaluation;

namespace hasm.Parsing.Models
{
    public sealed class ALU
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string Target { get; set; }
        public string Left { get; set; }
        public string Right { get; set; }
        public bool Carry { get; set; }
        public AluOperation Operation { get; set; }

        public static ALU Parse(string input)
        {
            input = new Regex("\\s+").Replace(input, "");
            if (string.IsNullOrEmpty(input))
                    throw new ArgumentNullException(nameof(input));

            var parsed = MicroHasmGrammar.Alu.ParseTree(input);

            var target = parsed.FirstValueByNameOrDefault<string>("target");
            var left = parsed.FirstValueByNameOrDefault<string>("left");
            var op = parsed.FirstValueByNameOrDefault<string>("op");
            var right = parsed.FirstValueByNameOrDefault<string>("right");
            var carry = parsed.FirstValueByNameOrDefault<string>("carry");
            var sp = parsed.FirstValueByNameOrDefault<string>("SP");
            var shift = parsed.FirstValueByNameOrDefault<string>("shift");
            var status = parsed.FirstValueByNameOrDefault<string>("status");
            var cond = parsed.FirstValueByNameOrDefault<string>("cond");
            var nop = parsed.FirstValueByNameOrDefault<string>("nop");

            return null;
        }
    }

    public enum AluOperation
    {
    }
}