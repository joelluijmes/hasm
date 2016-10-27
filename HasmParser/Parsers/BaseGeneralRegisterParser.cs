using System.Linq;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsers
{
	internal abstract class BaseGeneralRegisterParser : BaseRegisterParser
	{
		protected BaseGeneralRegisterParser(string name, char mask) : base(name, mask)
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