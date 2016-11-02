using System;
using System.Linq;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using ParserLib.Evaluation;
using ParserLib.Evaluation.Rules;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Parsers
{
    public sealed class OperandParser
    {
        private readonly ValueRule<string> _encodingRule;
        private readonly OperandEncoding _operandEncoding;
        private readonly ValueRule<int> _valueRule;
        private Rule _rule;

        public OperandParser()
        {
        }

        private OperandParser(ValueRule<int> valueRule, OperandEncoding operandEncoding)
        {
            _valueRule = valueRule;
            _operandEncoding = operandEncoding;
            _encodingRule = HasmGrammar.CreateMaskRule(operandEncoding.EncodingMask);
        }

        public string[] Operands => _operandEncoding.Operands;

        public static OperandParser Create(OperandEncoding operandEncoding)
        {
            if (operandEncoding == null)
                throw new ArgumentNullException(nameof(operandEncoding));

            var rule = operandEncoding.KeyValue != null
                ? Grammar.Or(operandEncoding.KeyValue.Select(keyValue => Grammar.KeyValue(keyValue)))
                : Grammar.Range(operandEncoding.Minimum, operandEncoding.Maximum, Grammar.Int32());

            var valueRule = Grammar.FirstValue<int>(rule);
            return new OperandParser(valueRule, operandEncoding);
        }

        public Rule CreateRule(string encoding)
        {
            if (_rule != null)
                return _rule;

            Func<string, int> converter = match =>
            {
                var value = _valueRule.FirstValue(match);
                return Encode(encoding, value);
            };

            _rule = Grammar.ConvertToValue(converter, _valueRule);
            return _rule;
        }

        private int Encode(string encoding, int value)
        {
            // TODO: make sure that value repalces the mask (i.e. masked encoding isn't required to be after each other)
            var opcodeBinary = _encodingRule.FirstValue(encoding); // gets the binary representation of the encoding
            var index = opcodeBinary.IndexOf(_operandEncoding.EncodingMask); // finds the first occurance of the mask
            var nextIndex = opcodeBinary.IndexOf('0', index); // and the last
            if (nextIndex == -1)
                nextIndex = opcodeBinary.Length; // could be that it ended with the mask so we set it to the length of total encoding

            var length = nextIndex - index;
            var bin = Convert.ToString(value, 2).PadLeft(_operandEncoding.Size, '0');

            opcodeBinary = opcodeBinary.Remove(index, length).Insert(index, bin);
            var result = Convert.ToInt32(opcodeBinary, 2);

            return result;
        }
    }
}