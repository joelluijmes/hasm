using System;
using System.Linq;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using OfficeOpenXml;
using ParserLib.Evaluation;
using ParserLib.Evaluation.Rules;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.OperandParsers
{
    public sealed class ExcelOperandParser : IOperandParser
    {
        private readonly Rule _matchRule;
        private readonly ValueRule<int> _valueRule;
        private readonly char _mask;
        private readonly ValueRule<string> _encodingRule;
        private Rule _rule;
        private int _size;

        public ExcelOperandParser()
        {
        }

        private ExcelOperandParser(Rule matchRule, ValueRule<int> valueRule, ExcelOperand operand)
        {
            _matchRule = matchRule;
            _valueRule = valueRule;
            OperandTypes = operand.Operands;
            _mask = operand.EncodingMask;
            _encodingRule = HasmGrammar.CreateMaskRule(_mask);
            _size = operand.Size;
        }

        public static ExcelOperandParser Create(ExcelOperand operand)
        {
            if (operand == null)
                throw new ArgumentNullException(nameof(operand));

            var matchRule = Grammar.MatchAnyString(operand.Operands);
            var rule = Grammar.Or(operand.KeyValue.Select(keyValue => Grammar.KeyValue(keyValue)));
            
            var valueRule = Grammar.FirstValue<int>(rule);


            return new ExcelOperandParser(matchRule, valueRule, operand);
        }

        public string[] OperandTypes { get; set; }

        public OperandType OperandType { get; }
        public Rule CreateRule(string encoding)
        {
            if (_rule != null)
                return _rule;

            Func<string, int> converter = match =>
            {
                var value = _valueRule.FirstValue(match);
                return Encode(encoding, value);
            };

            return Grammar.ConvertToValue(converter, _valueRule);
        }
        
        private int Encode(string encoding, int value)
        {
            // TODO: make sure that value repalces the mask (i.e. masked encoding isn't required to be after each other)
            var opcodeBinary = _encodingRule.FirstValue(encoding); // gets the binary representation of the encoding
            var index = opcodeBinary.IndexOf(_mask); // finds the first occurance of the mask
            var nextIndex = opcodeBinary.IndexOf('0', index); // and the last
            if (nextIndex == -1)
                nextIndex = opcodeBinary.Length; // could be that it ended with the mask so we set it to the length of total encoding

            var length = nextIndex - index;
            var bin = Convert.ToString(value, 2).PadLeft(_size, '0');

            opcodeBinary = opcodeBinary.Remove(index, length).Insert(index, bin);
            var result = Convert.ToInt32(opcodeBinary, 2);

            return result;
        }
    }
}