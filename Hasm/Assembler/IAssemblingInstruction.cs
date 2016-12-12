using hasm.Parsing.Export;

namespace hasm.Assembler
{
    internal interface IAssemblingInstruction : IAssembled
    {
        bool FullyAssembled { get; }
    }
}
