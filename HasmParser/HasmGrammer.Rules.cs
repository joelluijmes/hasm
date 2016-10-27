using ParserLib.Evaluation.Rules;

namespace hasm.Parsing
{
	public sealed partial class HasmGrammar
	{
		public static readonly ValueRule<string> Operand = Text("operand", MatchWhile(Label));
		public static readonly ValueRule<string> ListingLabel = FirstValue<string>("label", Text(MatchWhile(Label)) + MatchChar(':'));

	}
}