using hasm.Parsing.Export;

namespace hasm.Assembler
{
    internal sealed class AssembledLine : IAssembled
    {
        public int Address { get; set; }
        public int Count { get; }
        public long Assembled { get; }
    }
}
