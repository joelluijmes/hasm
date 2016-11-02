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
            _logger.Debug($"{input}:{Environment.NewLine}{parsed.PrettyFormat()}");

            return null;
        }
    }

    public enum AluOperation
    {
    }
}