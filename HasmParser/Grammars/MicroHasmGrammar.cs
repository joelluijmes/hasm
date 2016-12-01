using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Grammars
{
    public sealed class MicroHasmGrammar : Grammar
    {
        public static readonly Rule SP = Optional(Text("SP", MatchString("SP", true)) + MatchChar('='));
        public static readonly Rule Target = Text("target", Label) + MatchChar('=');
        public static readonly Rule MultiTarget = (Target + SP) | (SP + Target);
        public static readonly Rule Left = Text("left", Label | Int32());
        public static readonly Rule AluOperation = Text("op", MatchAnyString("+ - & | ^"));
        public static readonly Rule Right = Text("right", Label | Int32());
        public static readonly Rule Carry = Text("carry", PlusOrMinus + MatchChar('c', true));
        public static readonly Rule If = Node("if", MatchString("if", true) + Text("status", Label) + MatchChar('=') + Text("cond", MatchChar('1') | MatchChar('0')) + MatchChar(':'));
        public static readonly Rule Nop = Text("nop", MatchString("nop", true) | End());
        public static readonly Rule Shift = Text("lshift", MatchString(">>")) + MatchChar('1') | Text("ashift", MatchString(">>>")) + MatchChar('1');  // Left shift is implemented as DST + DST

        public static readonly Rule Alu = Node("alu", (MultiTarget + Left + Optional((AluOperation + Right + Optional(Carry)) | Shift)) | (Left + AluOperation + Right + Optional(Carry)) | Right);

        public static readonly Rule Operation = Optional(If) + (Nop  | Alu);
    }
}