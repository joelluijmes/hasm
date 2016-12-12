using System.Collections.Generic;

namespace hasm.Assembler.Directives
{
    internal interface IDirective
    {
        DirectiveTypes DirectiveType { get; }

        IList<IAssemblingInstruction> Parse(Line line, ref int address);
    }

    internal abstract class BaseDirective : IDirective
    {
        protected readonly IDictionary<string, string> Lookup;

        protected BaseDirective(IDictionary<string, string> lookup)
        {
            Lookup = lookup;
        }

        public abstract DirectiveTypes DirectiveType { get; }
        public abstract IList<IAssemblingInstruction> Parse(Line line, ref int address);
    }
}
