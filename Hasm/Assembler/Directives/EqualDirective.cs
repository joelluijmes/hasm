using System.Collections.Generic;
using hasm.Exceptions;
using hasm.Parsing.Grammars;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace hasm.Assembler.Directives
{
    internal sealed class EqualDirective : BaseDirective
    {
        public EqualDirective(IDictionary<string, string> lookup) : base(lookup) {}
        public override DirectiveTypes DirectiveType => DirectiveTypes.EQU;

        public override IList<IAssemblingInstruction> Parse(Line line, ref int address)
        {
            if (HasmGrammar.DirectiveEqual.Match(line.Operands))
            {
                var label = HasmGrammar.DirectiveEqual.FirstValueByName<string>(line.Operands, "label");
                var value = HasmGrammar.DirectiveEqual.FirstValueByName<int>(line.Operands, "value");

                Lookup[label] = value.ToString();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(line.Label))
                    throw new AssemblerException("Invalid input, when using the EQU directive a label should be specified or the format must be 'key=value'");

                if (!Grammar.Integer.Match(line.Operands))
                    throw new AssemblerException("Value must be a numeric value.");

                Lookup[line.Label] = line.Operands;
            }
            
            return null;
        }
    }
}
