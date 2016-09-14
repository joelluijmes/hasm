namespace MicParser.NodeTypes
{
    public enum LeftRegister : long
    {
        Null = 2 << 18,
        One  = 1 << 18,
        H    = 0 << 18
    }
}