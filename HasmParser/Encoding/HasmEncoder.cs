using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using hasm.Parsing.Parsers;
using hasm.Parsing.Parsers.Sheet;
using NLog;
using ParserLib.Evaluation;
using ParserLib.Evaluation.Rules;

namespace hasm.Parsing.Encoding
{
    /// <summary>
    ///     This class makes it possible to parse an instruction to an encoding as specified
    ///     in the HasmGrammar.
    /// </summary>
    public sealed class HasmEncoder 
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly IProvider<InstructionEncoding> _encodingProvider = new EncodingSheetProvider();

        private readonly HasmGrammar _grammar;
        private readonly IDictionary<string, ValueRule<byte[]>> _rules;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HasmEncoder" /> class.
        /// </summary>
        /// <param name="grammar">The grammar of the parser.</param>
        public HasmEncoder(HasmGrammar grammar)
        {
            _grammar = grammar;
            _rules = new ConcurrentDictionary<string, ValueRule<byte[]>>();
            //_logger.Info($"Learned {Items.Count} instructions");
        }

        /// <summary>
        ///     Encodes the specified input.
        /// </summary>
        /// <param name="input">The instruction to be parsed.</param>
        /// <returns>Parsed instruction</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public byte[] Encode(string input)
        {
            byte[] encoded;
            if (!TryEncode(input, out encoded))
                throw new NotImplementedException();

            return encoded;
        }

        /// <summary>
        ///     Tries to encode the input, if partly-failed (due label) the encoded will still be created
        ///     as an array of the expected length.
        /// </summary>
        /// <param name="input">The input to be encoded.</param>
        /// <param name="encoded">The encoded instruction.</param>
        /// <returns>True if succeeds</returns>
        /// <exception cref="System.ArgumentNullException">input</exception>
        public bool TryEncode(string input, out byte[] encoded)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));

            input = FormatInput(input);
            var opcode = HasmGrammar.Opcode.FirstValue(input);

            var rule = FindRule(opcode);
            if (rule == null)
            {
                encoded = null;
                return false;
            }

            if (rule.Match(input))
            {
                encoded = rule.FirstValue(input);
                return true;
            }

            var instruction = FindInstructionEncoding(opcode);
            encoded = new byte[instruction.Count];
            return false;
        }

        private ValueRule<byte[]> FindRule(string opcode)
        {
            ValueRule<byte[]> rule;
            if (_rules.TryGetValue(opcode, out rule))
            {
                _logger.Debug(() => $"Found rule: {rule}");
                return rule;
            }

            var instruction = FindInstructionEncoding(opcode);
            if (instruction == null)
                return null;

            rule = _grammar.ParseInstruction(instruction);
            _logger.Debug(() => $"Created rule: {rule}");

            _rules[opcode] = rule;
            return rule;
        }

        private InstructionEncoding FindInstructionEncoding(string opcode)
            => _encodingProvider.Items.FirstOrDefault(i => i.Grammar.StartsWith(opcode.ToUpper()));

        private static string FormatInput(string input)
        {
            var opcode = input.Split(' ')[0];
            var operands = input.Substring(opcode.Length);
            operands = string.Join("", operands.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
            operands = operands.Replace("+-", "-");

            return opcode + " " + operands;
        }
    }
}
