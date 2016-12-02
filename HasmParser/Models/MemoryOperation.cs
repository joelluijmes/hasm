namespace hasm.Parsing.Models
{
    public enum MemoryOperation
    {
        None,
        ReadCode = 1 << 0,
        Write = 1 << 1,
        ReadData = 1 << 2
    }
}
