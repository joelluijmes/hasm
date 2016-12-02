namespace hasm.Parsing.Models
{
    public enum AluOperation
    {
        Clear = 0,
        Minus = 1 << 1,
        InverseMinus = 1 << 0,
        Plus = (1 << 1) | (1 << 0),
        Xor = 1 << 2,
        Or = (1 << 2) | (1 << 0),
        And = (1 << 2) | (1 << 1),
        Preset = (1 << 2) | (1 << 1) | (1 << 0)
    }
}
