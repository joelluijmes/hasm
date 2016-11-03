using hasm.Parsing.Models;
using ParserLib.Evaluation.Rules;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Grammars
{
    public sealed class MicroHasmGrammar : Grammar
    {
        public static readonly Rule SP = Optional(Text("SP", MatchString("SP", true)) + MatchChar('='));
        public static readonly Rule Target = Text("target", Label) + MatchChar('=');
        public static readonly Rule MultiTarget = (Target + SP) | (SP + Target);
        public static readonly ValueRule<string> Left = Text("left", Label | Int32());
        public static readonly ValueRule<string> Operation = Text("op", MatchAnyString("+ - & | ^"));
        public static readonly ValueRule<string> Right = Text("right", Label | Int32());
        public static readonly ValueRule<string> Carry = Text("carry", PlusOrMinus + MatchChar('c', true));
        public static readonly Rule If = MatchString("if", true) + Text("status", Label) + MatchChar('=') + Text("cond", MatchChar('1') | MatchChar('0')) + MatchChar(':');
        public static readonly Rule Nop = Text("nop", MatchString("nop", true) | End());
        public static readonly Rule Shift = Text("shift", MatchString("<<") | MatchString(">>")) + MatchChar('1');

        public static readonly Rule Alu = (MultiTarget + Left + Optional(Operation + Right + Optional(Carry) | Shift)) | (Left + Operation + Right + Optional(Carry));
    }
}