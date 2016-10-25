using System.Linq;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Parsers
{
	internal abstract class BaseRegisterParser : BaseParser
	{
		private const int SIZE_REGISTER_ENCODING = 3;

		protected BaseRegisterParser(string name, char mask) : base(name, mask, SIZE_REGISTER_ENCODING)
		{
		}

		protected override Rule CreateMatchRule()
		{
			var range = Enumerable.Range(0, 8) // generate 0 .. 7
				.Select(i => i.ToString()[0]) // convert them to strings
				.Select(i => Grammar.ConvertToValue(NumberConverter, Grammar.MatchChar(i))); // apply the NumberConverter

			return Grammar.MatchChar('R', true) + Grammar.Or(range);
		}
	}
}