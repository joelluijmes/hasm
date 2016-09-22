using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace MicParser.Grammars
{
    public sealed class AssemblerGrammar : Grammar
    {
        public static readonly Rule Section = FirstValue<string>("Section", MatchString("section", true) + MatchChar('.') + Text("name", Label));
    }
}