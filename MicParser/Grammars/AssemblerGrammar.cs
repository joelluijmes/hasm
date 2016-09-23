using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace MicParser.Grammars
{
    public sealed class AssemblerGrammar : Grammar
    {
        private static readonly Rule _anyChar = Char(c => true);
        private static readonly Rule _newline = MatchChar('\r').Optional + MatchChar('\n');
        private static readonly Rule _whitespace = OneOrMore(Char(char.IsWhiteSpace));      // one or more whitespace
        private static Rule _whileNot(Rule rule) => ZeroOrMore(rule.Not + _anyChar);

        public static readonly Rule Section = FirstValue<string>("Section", MatchString("section", true) + _whitespace + MatchChar('.') + Text("name", Label));

        public static readonly Rule BIPUSH = EnumValue<Mnemonic, int>(Mnemonic.BIPUSH) + Whitespace.Optional + Int8("byte");
        public static readonly Rule DUP = EnumValue<Mnemonic, int>(Mnemonic.DUP);
        public static readonly Rule GOTO = EnumValue<Mnemonic, int>(Mnemonic.GOTO) + Whitespace.Optional + (Int16("absolute") | Text("label", Label));
        public static readonly Rule IADD = EnumValue<Mnemonic, int>(Mnemonic.IADD);
        public static readonly Rule IAND = EnumValue<Mnemonic, int>(Mnemonic.IAND);
        public static readonly Rule IFEQ = EnumValue<Mnemonic, int>(Mnemonic.IFEQ) + Whitespace.Optional + (Int16("absolute") | Text("label", Label));
        public static readonly Rule IFLT = EnumValue<Mnemonic, int>(Mnemonic.IFLT) + Whitespace.Optional + (Int16("absolute") | Text("label", Label));
        public static readonly Rule IF_ICMPEQ = EnumValue<Mnemonic, int>(Mnemonic.IF_ICMPEQ) + Whitespace.Optional + (Int16("absolute") | Text("label", Label));
        public static readonly Rule IINC = EnumValue<Mnemonic, int>(Mnemonic.IINC) + Whitespace.Optional + (Int8("varnum") | Text("var", Label)) + Whitespace.Optional + Int8("const");
        public static readonly Rule ILOAD = EnumValue<Mnemonic, int>(Mnemonic.ILOAD) + Whitespace.Optional + (Int8("varnum") | Text("var", Label));
        public static readonly Rule INVOKEVIRTUAL = EnumValue<Mnemonic, int>(Mnemonic.INVOKEVIRTUAL) + Whitespace.Optional + (Int16("absolute") | Text("label", Label));
        public static readonly Rule IOR = EnumValue<Mnemonic, int>(Mnemonic.IOR);
        public static readonly Rule IRETURN = EnumValue<Mnemonic, int>(Mnemonic.IRETURN);
        public static readonly Rule ISTORE = EnumValue<Mnemonic, int>(Mnemonic.ISTORE) + Whitespace.Optional + (Int8("varnum") | Text("var", Label));
        public static readonly Rule ISUB = EnumValue<Mnemonic, int>(Mnemonic.ISUB);
        public static readonly Rule LDC_W = EnumValue<Mnemonic, int>(Mnemonic.LDC_W) + Whitespace.Optional + (Int16("absolute") | Text("label", Label));
        public static readonly Rule NOP = EnumValue<Mnemonic, int>(Mnemonic.NOP);
        public static readonly Rule POP = EnumValue<Mnemonic, int>(Mnemonic.POP);
        public static readonly Rule SWAP = EnumValue<Mnemonic, int>(Mnemonic.SWAP);
        public static readonly Rule WIDE = EnumValue<Mnemonic, int>(Mnemonic.WIDE);
    }
}