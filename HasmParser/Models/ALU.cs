namespace hasm.Parsing.Models
{
    public sealed class ALU
    {
        public string Target { get; set; }
        public string Left { get; set; }
        public string Right { get; set; }
        public bool Carry { get; set; }
        public AluOperation Operation { get; set; }

        public static ALU Parse(string aluString)
        {
            return null;
        }
    }

    public enum AluOperation
    {
    }
}