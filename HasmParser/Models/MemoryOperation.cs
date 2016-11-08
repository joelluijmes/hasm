namespace hasm.Parsing.Models
{
    public enum MemoryOperation
    {
        None,
        Read = 1 << 0,
        Write = 1 << 1
    }
}