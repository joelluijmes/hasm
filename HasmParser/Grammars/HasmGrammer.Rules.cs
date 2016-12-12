using ParserLib.Evaluation.Rules;
using ParserLib.Parsing.Rules;

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
        public static readonly ValueRule<string> AssemblyLabel = FirstValue<string>("label", Text(Label) + MatchChar(':'));

        public static readonly ValueRule<string> AssemblyDirective = FirstValue<string>("directive", MatchChar('.') + Text(Label));

        /// <summary>
        ///     The listing instruction, just about anything till the end or ';'
        /// </summary>
        public static readonly ValueRule<string> AssemblyInstruction = FirstValue<string>("instruction", Text(MatchWhile(Not(MatchChar(';')) + MatchAnyChar())));

        public static readonly ValueRule<string> Operands = FirstValue<string>("operands", Text(MatchWhile(Not(MatchChar(';')) + MatchAnyChar())));

        /// <summary>
        ///     The listing comment, must start with ';', matches till the end.
        /// </summary>
        public static readonly ValueRule<string> AssemblyComment = FirstValue<string>("comment", MatchChar(';') + Text(MatchWhile(MatchAnyChar())));

        public static readonly Rule Line = (Optional(AssemblyLabel) + Optional(Whitespace) + (AssemblyDirective | AssemblyInstruction) + Optional(Whitespace) + Optional(Operands) + Optional(Whitespace) + Optional(AssemblyComment)) | AssemblyComment | End();

        public static readonly Rule DirectiveEqual = Text("label", Label) + MatchChar('=') + Int32("value");

        public static readonly Rule DefineByte = Int8() + Optional(OneOrMore(MatchChar(',') + Optional(Whitespace) + Int8()));
    }
}
