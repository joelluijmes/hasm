namespace hasm.Parsing.Export
{
    public interface IAssembled
    {
        int Address { get; set; }
        int Count { get; }
        long Assembled { get; }
    }
}
