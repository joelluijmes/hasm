using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.OperandParsers
{
	/// <summary>
	/// Provides base class for immediates
	/// </summary>
	/// <seealso cref="BaseOperandParser" />
	internal abstract class BaseImmediateParser : BaseOperandParser
	{
		private const char MASK = 'k';

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseImmediateParser"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="size">The amount of bits in the encoding.</param>
		protected BaseImmediateParser(string name, int size) : base(name, MASK, size)
		{
		}

		/// <summary>
		/// Creates the match rule for immediates.
		/// </summary>
		/// <returns>
		/// The rule.
		/// </returns>
		protected override Rule CreateMatchRule() => Grammar.ConvertToValue(NumberConverter, (Grammar.MatchChar('-') | Grammar.MatchChar('+').Optional) + Grammar.Digits);
	}
}