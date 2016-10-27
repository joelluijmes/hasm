using ParserLib.Evaluation.Rules;

namespace hasm.Parsing
{
	public sealed partial class HasmGrammar
	{
		public static readonly ValueRule<string> Opcode = Text("opcode", Label);
		public static readonly ValueRule<string> ListingLabel = FirstValue<string>("label", Text(Label) + MatchChar(':'));
		public static readonly ValueRule<string> ListingInstruction = FirstValue<string>("instruction", Text(MatchWhile(MatchChar(';').Not + MatchAnyChar())));
		public static readonly ValueRule<string> ListingComment = FirstValue<string>("comment", MatchChar(';') + Text(MatchWhile(MatchAnyChar())));
	}
}