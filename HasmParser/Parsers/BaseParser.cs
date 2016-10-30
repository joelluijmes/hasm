using System;
using NLog;
using ParserLib.Evaluation;
using ParserLib.Evaluation.Rules;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Parsers
{
	internal abstract class BaseParser : IParser
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly ValueRule<string> _encodingMask;

		protected readonly char Mask;
		protected readonly string Name;
		protected readonly int Size;

		private Rule _rule;

		protected BaseParser(string name, char mask, int size)
		{
			Name = name;
			Mask = mask;
			Size = size;
			_encodingMask = HasmGrammar.CreateMaskRule(mask);
		}

		public abstract OperandType OperandType { get; }

		public Rule CreateRule(string encoding)
		{
			if (_rule != null)
				return _rule;

			var matchRule = Grammar.FirstValue<string>(CreateMatchRule());
			Func<string, int> converter = match =>
			{
				var value = matchRule.FirstValue(match);
				return Encode(encoding, value);
			};

			_rule = Grammar.ConvertToValue(Name, converter, matchRule);
			_logger.Debug($"Created parser for {Name}");

			return _rule;
		}

		protected abstract Rule CreateMatchRule();

		protected string NumberConverter(string value)
		{
			var number = int.Parse(value);
			var binary = Convert.ToString(number, 2).PadLeft(Size, '0');
			return binary.Substring(0, Math.Min(Size, binary.Length));
		}

		private int Encode(string encoding, string value)
		{
			var opcodeBinary = _encodingMask.FirstValue(encoding); // gets the binary representation of the encoding
			var index = opcodeBinary.IndexOf(Mask); // finds the first occurance of the mask
			var nextIndex = opcodeBinary.IndexOf('0', index); // and the last
			if (nextIndex == -1)
				nextIndex = opcodeBinary.Length; // could be that it ended with the mask so we set it to the length of total encoding

			var length = nextIndex - index;
			opcodeBinary = opcodeBinary.Remove(index, length).Insert(index, value);
			var result = Convert.ToInt32(opcodeBinary, 2);

			return result;
		}
	}
}