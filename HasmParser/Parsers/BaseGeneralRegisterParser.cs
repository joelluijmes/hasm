using System.Linq;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Parsers
{
	/// <summary>
	/// Provides base for general register parsers
	/// </summary>
	/// <seealso cref="hasm.Parsing.Parsers.BaseRegisterParser" />
	internal abstract class BaseGeneralRegisterParser : BaseRegisterParser
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseGeneralRegisterParser"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="mask">The mask in the encoding.</param>
		protected BaseGeneralRegisterParser(string name, char mask) : base(name, mask)
		{
		}

		/// <summary>
		/// Creates the match rule for the grammar.
		/// </summary>
		/// <returns>
		/// The rule.
		/// </returns>
		protected override Rule CreateMatchRule()
		{
			var range = Enumerable.Range(0, 8) // generate 0 .. 7
				.Select(i => i.ToString()[0]) // convert them to strings
				.Select(i => Grammar.ConvertToValue(NumberConverter, Grammar.MatchChar(i))); // apply the NumberConverter

			return Grammar.MatchChar('R', true) + Grammar.Or(range);
		}
	}
}