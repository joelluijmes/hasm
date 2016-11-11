namespace hasm.Parsing.Models
{
    public enum MemoryOperation
    {
        None,
        Write = 1 << 0,
        Read = 1 << 1
    }
}