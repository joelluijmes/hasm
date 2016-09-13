namespace MicParser.NodeTypes
{
    public enum MemoryOperation : long
    {
        wr = 1L << 29,
        rd = 1L << 30,
        fetch = 1L << 31
    }
}