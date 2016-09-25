using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace MicParser.Grammars
{
    public sealed class AssemblerGrammar : Grammar
    {
        private static readonly Rule _whitespace = OneOrMore(Char(char.IsWhiteSpace)); // one or more whitespace

        public static readonly Rule Definition = FirstValue<string>("Definition", MatchString(".def", true) + _whitespace + Text("key", Label) + _whitespace + Text("value", Label));
        public static readonly Rule Section = FirstValue<string>("Section", MatchString("section", true) + _whitespace + MatchChar('.') + Text("name", Label));

        public static readonly Rule Byte = Int8("byte");
        public static readonly Rule Varnum = Int8("varnum");
        public static readonly Rule Const = Int8("const");
        public static readonly Rule Var = Text("var", Label);
        public static readonly Rule BranchLabel = Text("label", Label);

        public static readonly Rule BIPUSH = EnumValue<Mnemonic, byte>(Mnemonic.BIPUSH) + Whitespace.Optional + Byte;
        public static readonly Rule DUP = EnumValue<Mnemonic, byte>(Mnemonic.DUP);
        public static readonly Rule GOTO = EnumValue<Mnemonic, byte>(Mnemonic.GOTO) + Whitespace.Optional + (Int16("absolute") | BranchLabel);
        public static readonly Rule IADD = EnumValue<Mnemonic, byte>(Mnemonic.IADD);
        public static readonly Rule IAND = EnumValue<Mnemonic, byte>(Mnemonic.IAND);
        public static readonly Rule IFEQ = EnumValue<Mnemonic, byte>(Mnemonic.IFEQ) + Whitespace.Optional + (Int16("absolute") | BranchLabel);
        public static readonly Rule IFLT = EnumValue<Mnemonic, byte>(Mnemonic.IFLT) + Whitespace.Optional + (Int16("absolute") | BranchLabel);
        public static readonly Rule IF_ICMPEQ = EnumValue<Mnemonic, byte>(Mnemonic.IF_ICMPEQ) + Whitespace.Optional + (Int16("absolute") | BranchLabel);
        public static readonly Rule IINC = EnumValue<Mnemonic, byte>(Mnemonic.IINC) + Whitespace.Optional + (Varnum | Var) + Whitespace.Optional + Const;
        public static readonly Rule ILOAD = EnumValue<Mnemonic, byte>(Mnemonic.ILOAD) + Whitespace.Optional + (Varnum | Var);
        public static readonly Rule INVOKEVIRTUAL = EnumValue<Mnemonic, byte>(Mnemonic.INVOKEVIRTUAL) + Whitespace.Optional + (Int16("absolute") | BranchLabel);
        public static readonly Rule IOR = EnumValue<Mnemonic, byte>(Mnemonic.IOR);
        public static readonly Rule IRETURN = EnumValue<Mnemonic, byte>(Mnemonic.IRETURN);
        public static readonly Rule ISTORE = EnumValue<Mnemonic, byte>(Mnemonic.ISTORE) + Whitespace.Optional + (Varnum | Var);
        public static readonly Rule ISUB = EnumValue<Mnemonic, byte>(Mnemonic.ISUB);
        public static readonly Rule LDC_W = EnumValue<Mnemonic, byte>(Mnemonic.LDC_W) + Whitespace.Optional + (Int16("absolute") | BranchLabel);
        public static readonly Rule NOP = EnumValue<Mnemonic, byte>(Mnemonic.NOP);
        public static readonly Rule POP = EnumValue<Mnemonic, byte>(Mnemonic.POP);
        public static readonly Rule SWAP = EnumValue<Mnemonic, byte>(Mnemonic.SWAP);
        public static readonly Rule WIDE = EnumValue<Mnemonic, byte>(Mnemonic.WIDE);

        public static readonly Rule Instruction = BIPUSH | DUP | GOTO | IADD | IAND | IFEQ | IFLT | IF_ICMPEQ | IINC | ILOAD | INVOKEVIRTUAL | IOR | IRETURN | ISTORE | ISUB | LDC_W | NOP | POP | SWAP | WIDE;

        public static readonly Rule DataInt8 = BranchLabel + MatchChar(':') + _whitespace + MatchString("db", true) + _whitespace + Byte;
    }
}