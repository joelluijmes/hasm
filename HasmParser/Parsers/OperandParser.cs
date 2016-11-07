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
        public ValueRule<string> EncodingRule { get; }
        public OperandEncoding OperandEncoding { get; }
        public ValueRule<int> ValueRule { get; }

        private OperandParser(ValueRule<int> valueRule, OperandEncoding operandEncoding)
        {
            ValueRule = valueRule;
            OperandEncoding = operandEncoding;
            EncodingRule = HasmGrammar.CreateMaskRule(operandEncoding.EncodingMask);
        }

        public string[] Operands => OperandEncoding.Operands;

        public static OperandParser Create(OperandEncoding operandEncoding)
        {
            if (operandEncoding == null)
                throw new ArgumentNullException(nameof(operandEncoding));

            Rule rule;
            switch (operandEncoding.Type)
            {
                case OperandEncodingType.KeyValue:
                    rule = Grammar.Or(operandEncoding.Pairs.Select(keyValue => Grammar.KeyValue(keyValue)));
                    break;
                case OperandEncodingType.Range:
                    rule = Grammar.Range(operandEncoding.Minimum, operandEncoding.Maximum, Grammar.Int32());
                    break;

                case OperandEncodingType.Aggregation:
                    var operands = operandEncoding.Operands.Single().Split(new[] {'+', ' '}, StringSplitOptions.RemoveEmptyEntries);
                    var rules = operands.Select(HasmGrammar.FindOperandParser).Select(o => o.ValueRule);

                    rule = Grammar.Sequence(rules);
                    break;

                default:
                    throw new NotImplementedException();
            }
            
            var valueRule = Grammar.FirstValue<int>(rule);
            return new OperandParser(valueRule, operandEncoding);
        }

        public Rule CreateRule(string encoding)
        {
            if (OperandEncoding.Type != OperandEncodingType.Aggregation)
                return CreateConverterRule(ValueRule, encoding);

            var operands = OperandEncoding.Operands.Single().Split(new[] { '+', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var rules = operands.Select(HasmGrammar.FindOperandParser).Select(o => o.CreateRule(encoding));
            return Grammar.Sequence(rules);
        }

        private Rule CreateConverterRule(ValueRule<int> rule, string encoding)
        {
            Func<string, int> converter = match =>
            {
                var value = rule.FirstValue(match);
                return Encode(encoding, value);
            };

            return Grammar.ConvertToValue(converter, rule);
        }

        private int Encode(string encoding, int value)
        {
            // TODO: make sure that value repalces the mask (i.e. masked encoding isn't required to be after each other)
            var opcodeBinary = EncodingRule.FirstValue(encoding); // gets the binary representation of the encoding
            var index = opcodeBinary.IndexOf(OperandEncoding.EncodingMask); // finds the first occurance of the mask
            var nextIndex = opcodeBinary.IndexOf('0', index); // and the last
            if (nextIndex == -1)
                nextIndex = opcodeBinary.Length; // could be that it ended with the mask so we set it to the length of total encoding

            var length = nextIndex - index;
            var bin = Convert.ToString(value, 2).PadLeft(OperandEncoding.Size, '0');

            opcodeBinary = opcodeBinary.Remove(index, length).Insert(index, bin);
            var result = Convert.ToInt32(opcodeBinary, 2);

            return result;
        }
    }
}