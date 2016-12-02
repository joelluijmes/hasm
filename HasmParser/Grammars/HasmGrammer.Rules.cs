using ParserLib.Evaluation.Rules;

namespace hasm.Parsing.Grammars
{
    public sealed partial class HasmGrammar
    {
        /// <summary>
        ///     Rule for opcode.
        /// </summary>
        public static readonly ValueRule<string> Opcode = Text("opcode", Label);

        /// <summary>
        ///     Rule for label defined as label + ':'
        /// </summary>
        public static readonly ValueRule<string> ListingLabel = FirstValue<string>("label", Text(Label) + MatchChar(':'));

        /// <summary>
        ///     The listing instruction, just about anything till the end or ';'
        /// </summary>
        public static readonly ValueRule<string> ListingInstruction = FirstValue<string>("instruction", Text(MatchWhile(MatchChar(';').Not + MatchAnyChar())));

        /// <summary>
        ///     The listing comment, must start with ';', matches till the end.
        /// </summary>
        public static readonly ValueRule<string> ListingComment = FirstValue<string>("comment", MatchChar(';') + Text(MatchWhile(MatchAnyChar())));
    }
}
