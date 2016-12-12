using System.Collections.Generic;
using hasm.Parsing.Grammars;
using ParserLib.Evaluation;

namespace hasm.Assembler.Directives
{
    internal sealed class EqualDirective : BaseDirective
    {
        public EqualDirective(IDictionary<string, string> lookup) : base(lookup) {}
        public override DirectiveTypes DirectiveType => DirectiveTypes.EQU;

        public override IList<IAssemblingInstruction> Parse(Line line, ref int address)
        {
            var label = HasmGrammar.DirectiveEqual.FirstValueByName<string>(line.Operands, "label");
            var value = HasmGrammar.DirectiveEqual.FirstValueByName<int>(line.Operands, "value");

            Lookup[label] = value.ToString();

            return null;
        }
    }
}
