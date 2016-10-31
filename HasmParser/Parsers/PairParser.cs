using System.Linq;
using ParserLib.Evaluation;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

/// <summary>
/// 
/// </summary>
namespace hasm.Parsing.Parsers
{
	/// <summary>
	/// Parses an Y/Z pair
	/// </summary>
	/// <seealso cref="hasm.Parsing.Parsers.BaseParser" />
	internal sealed class PairParser : BaseParser
	{
		private const string NAME = "Y/Z";
		private const char MASK = 'p';
		private const int SIZE = 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="PairParser"/> class.
		/// </summary>
		public PairParser() : base(NAME, MASK, SIZE)
		{
		}

		/// <summary>
		/// Gets the type of the operand.
		/// </summary>
		/// <value>
		/// OperandType.Pair
		/// </value>
		public override OperandType OperandType => OperandType.Pair;

		/// <summary>
		/// Creates the match rule for the grammar.
		/// </summary>
		/// <returns>
		/// The rule.
		/// </returns>
		protected override Rule CreateMatchRule()
		{
			// TODO: from encoding sheet
			var y = Grammar.ConstantValue(0, Grammar.MatchChar('y', true));
			var z = Grammar.ConstantValue(1, Grammar.MatchChar('z', true));

			return Grammar.ConvertToValue(s =>
			{
				var value = s.Leafs.First().FirstValue<int>();
				return NumberConverter(value.ToString());
			}, y | z);
		}
	}
}