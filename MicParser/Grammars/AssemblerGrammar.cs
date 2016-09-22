using System;
using System.Collections.Generic;
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
        
        public static readonly Rule SectionName = FirstValue<string>("Name", MatchString("section", true) + _whitespace + MatchChar('.') + Text("name", Label));
        public static readonly Rule SectionContent = Text("Content", _whileNot(MatchString("section", true) | End()));
        public static readonly Rule Section = SectionName + _newline + SectionContent;

        public static readonly Rule Mnemonic = MatchEnum<Mnemonic, int>("Mnemonic");
    }
}