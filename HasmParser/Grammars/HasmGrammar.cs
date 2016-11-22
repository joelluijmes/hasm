using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using hasm.Parsing.DependencyInjection;
using hasm.Parsing.Models;
using hasm.Parsing.Providers;
using NLog;
using ParserLib.Evaluation;
using ParserLib.Evaluation.Rules;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Grammars
{
    /// <summary>
    ///     Provides type for defining the grammar with definitions for hasm
    /// </summary>
    /// <seealso cref="ParserLib.Parsing.Grammar" />
    public sealed partial class HasmGrammar : Grammar, IProvider<OperandParser>
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly IDictionary<char, ValueRule<string>> _maskRules;
        private static readonly ValueRule<string> _opcodemaskRule;
        private static readonly IList<OperandParser> _operandParsers;

        static HasmGrammar()
        {
            _maskRules = new Dictionary<char, ValueRule<string>>();
            _opcodemaskRule = CreateMaskRule('1');

            _operandParsers = new List<OperandParser>();
            var encodingProvider = KernelFactory.Resolve<IProvider<OperandEncoding>>();
            foreach (var encoding in encodingProvider.Items)
                _operandParsers.Add(OperandParser.Create(encoding));
        }
        
        public static OperandParser FindOperandParser(string operand)
        {
            if (operand == null)
                return null;

            foreach (var parser in _operandParsers)
            {
                if (parser.Operands.Contains(operand))
                    return parser;

                var encoding = parser.OperandEncoding;
                switch (encoding.Type)
                {
                case OperandEncodingType.KeyValue:
                    if (encoding.Pairs.Any(p => p.Key == operand))
                        return parser;

                    break;

                case OperandEncodingType.Range:
                    int operandAsNumber;
                    if (int.TryParse(operand, out operandAsNumber) || int.TryParse(operand.Replace("0x", ""), NumberStyles.HexNumber, null, out operandAsNumber))
                    {
                        if ((operandAsNumber >= encoding.Minimum) && (operandAsNumber <= encoding.Maximum))
                            return parser;
                    }

                    break;
                }
            }

            return null;
        }

        public static string[] GetOperands(string grammar)
        {
            var operand = Opcode.FirstValue(grammar);
            return grammar.Replace(operand, "") // remove operand from grammar
                          .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries) // operands are split with a ,
                          .Select(s => s.Trim())
                          .ToArray();
        }

        /// <summary>
        ///     Parses the instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <returns>Rule to conver the input to a byte[]</returns>
        internal ValueRule<byte[]> ParseInstruction(InstructionEncoding instruction)
        {
            var rule = ParseOpcode(instruction);
            var operands = ParseOperands(instruction);

            if (operands != null)
                rule += Whitespace + operands;

            Func<Node, byte[]> converter = node =>
            {
                var encodedAsInteger = node.FirstValue<int>();
                var encoded = BitConverter.GetBytes(encodedAsInteger);
                Array.Resize(ref encoded, instruction.Count);

                return encoded;
            };

            _logger.Debug($"Compiling rule for {instruction}: {instruction.Grammar}");
            return ConvertToValue(converter, Accumulate<int>((current, next) => current | next, rule));
        }

        /// <summary>
        ///     Creates a rule to mask encoding format.
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <returns>Rule which masks the encoding.</returns>
        internal static ValueRule<string> CreateMaskRule(char mask)
        {
            ValueRule<string> rule;
            if (_maskRules.TryGetValue(mask, out rule))
                return rule;

            var matched = ConstantValue(mask.ToString(), MatchChar(mask)); // matches only the mask
            var rest = ConstantValue("0", MatchAnyChar()); // treat the rest as an zero

            rule = Accumulate<string>((cur, next) => cur + next, MatchWhile(matched | rest)); // merge the encoding
            _logger.Debug($"Created encoding-mask ('{mask}') rule");
            _maskRules[mask] = rule;

            return rule;
        }

        private Rule ParseOperands(InstructionEncoding instruction)
        {
            var operands = GetOperands(instruction.Grammar);
            if (!operands.Any()) // instruction without oprands
                return null;

            return operands.Select(o => ParseOperand(o, instruction.Encoding)) // make operand rules from the strings
                           .Aggregate((total, next) => total + MatchChar(',') + next); // merge the rules sepearted by a ,
        }

        private Rule ParseOperand(string operand, string encoding)
        {
            var parser = FindOperandParser(operand);

            var rule = parser.CreateRule(encoding);
            rule.Name = operand; // give the name that was used to parse it :)

            return rule;
        }

        private static Rule ParseOpcode(InstructionEncoding instruction)
        {
            var opcode = Opcode.FirstValue(instruction.Grammar);
            var encoding = OpcodeEncoding(instruction.Encoding);
            return ConstantValue(encoding, MatchString(opcode, true)); // when it matches the opcode give its encoding 
        }

        private static int OpcodeEncoding(string encoding)
        {
            var opcodeBinary = _opcodemaskRule.FirstValue(encoding); // gets the binary representation of the encoding
            return Convert.ToInt32(opcodeBinary, 2);
        }

        public IList<OperandParser> Items => _operandParsers;
    }
}
