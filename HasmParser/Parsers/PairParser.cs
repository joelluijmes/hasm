using System.Linq;
using ParserLib.Evaluation;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Parsers
{
	internal sealed class PairParser : BaseParser
	{
		private const string NAME = "Y/Z";
		private const char MASK = 'p';
		private const int SIZE = 1;

		public PairParser() : base(NAME, MASK, SIZE)
		{
		}

		public override OperandType OperandType => OperandType.Pair;

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