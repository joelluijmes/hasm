namespace MicParser
{
    public struct MicroInstruction
    {
        public string Label { get; }
        public long Instruction { get; }

        public MicroInstruction(string label, long instruction)
        {
            Label = label;
            Instruction = instruction;
        }
    }
}