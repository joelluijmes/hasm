namespace hasm.Parsing.Export
{
    public interface IAssembled
    {
        int Address { get; }
        int Count { get; }
        long Assembled { get; }
    }
}
