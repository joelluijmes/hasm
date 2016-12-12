using hasm.Parsing.Export;

namespace hasm
{
    internal sealed class AssembledLine : IAssembled
    {
        public int Address { get; set; }
        public int Count { get; }
        public long Assembled { get; }
    }
}