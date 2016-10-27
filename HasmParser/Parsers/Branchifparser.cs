using System.Linq;
using ParserLib.Evaluation;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsers
{
	internal sealed class BranchIfparser : BaseParser
	{
		private const string NAME = "c";
		private const char MASK = 'c';
		private const int SIZE = 3;

		public BranchIfparser() : base(NAME, MASK, SIZE)
		{
		}

		public override OperandType OperandType => OperandType.BranchIf;

		protected override Rule CreateMatchRule()
		{
			// TODO: from encoding sheet
			var carry = Grammar.ConstantValue(0, Grammar.MatchChar('c', true));
			var zero = Grammar.ConstantValue(1, Grammar.MatchChar('z', true));
			var negative = Grammar.ConstantValue(2, Grammar.MatchChar('n', true));
			var overflow = Grammar.ConstantValue(3, Grammar.MatchChar('v', true));
			var sign = Grammar.ConstantValue(4, Grammar.MatchChar('s', true));

			return Grammar.ConvertToValue(s =>
			{
				var value = s.Leafs.First().FirstValue<int>();
				return NumberConverter(value.ToString());
			}, carry | zero | negative | overflow | sign);
		}
	}
}