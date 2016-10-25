using System;
using NLog;
using ParserLib;
using ParserLib.Evaluation;
using ParserLib.Evaluation.Rules;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Parsers
{
	internal abstract class BaseParser
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
			_encodingMask = HasmGrammer.CreateMaskRule(mask);
		}

		public abstract OperandType OperandType { get; }
		
		protected abstract Rule CreateMatchRule();

		protected string NumberConverter(string value)
		{
			var number = int.Parse(value); // convert it to a number
			return Convert.ToString(number, 2).PadLeft(Size, '0'); // then use Convert to make it binary
		}

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
			_logger.Debug(() => $"Created rule for {Name}: {_rule} with encoding {encoding}");
			_logger.Debug(() => $"MatchRule {matchRule.PrettyFormat()}");
			_logger.Debug(() => $"ConvertRule {_rule.PrettyFormat()}");

			return _rule;
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

			_logger.Info($"Encoding ({Mask}) for {encoding} is {opcodeBinary} ({result})");
			return result;
		}
	}
}