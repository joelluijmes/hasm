using hasm.Parsing.Models;
using ParserLib.Evaluation.Rules;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Grammars
{
    public sealed class MicroHasmGrammar : Grammar
    {
        public static readonly ValueRule<string> Target = FirstValue<string>(Text("target", Label) + MatchChar('='));
        public static readonly ValueRule<string> Left = Text("left", Label | Integer);
        public static readonly ValueRule<string> Operation = Text("op", MatchAnyString("+ - & | ^"));
        public static readonly ValueRule<string> Right = Text("right", Label);
        public static readonly ValueRule<string> Carry = Text("carry", PlusOrMinus + MatchChar('c', true));

        public static readonly Rule Alu = (Target + Left + Optional(Operation + Right + Optional(Carry))) | (Left + Operation + Right + Optional(Carry)) | MatchAnyChar();
    }
}