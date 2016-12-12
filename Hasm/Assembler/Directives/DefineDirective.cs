using System.Collections.Generic;
using hasm.Parsing.Grammars;
using ParserLib.Evaluation;

namespace hasm.Assembler.Directives
{
    internal sealed class DefineDirective : BaseDirective
    {
        public DefineDirective(IDictionary<string, string> lookup) : base(lookup) {}
        public override DirectiveTypes DirectiveType => DirectiveTypes.DEF;

        public override IList<IAssemblingInstruction> Parse(Line line, ref int address)
        {
            var label = HasmGrammar.DirectiveDefine.FirstValueByName<string>(line.Operands, "label");
            var value = HasmGrammar.DirectiveDefine.FirstValueByName<string>(line.Operands, "text");

            Lookup[label] = value;
            return null;
        }
    }
}
