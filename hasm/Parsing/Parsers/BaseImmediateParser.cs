using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Parsers
{
	internal abstract class BaseImmediateParser : BaseParser
	{
		private const char MASK = 'k';

		protected BaseImmediateParser(string name, int size) : base(name, MASK, size)
		{
		}

		protected override Rule CreateMatchRule() => Grammar.ConvertToValue(NumberConverter, Grammar.Digits);
	}
}