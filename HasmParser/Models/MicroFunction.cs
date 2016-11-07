using System.Collections.Generic;

namespace hasm.Parsing.Models
{
    public sealed class MicroFunction
    {
        public string Instruction { get; }
        public IList<MicroInstruction> MicroInstructions { get; }

        public MicroFunction(string instruction, IList<MicroInstruction> microInstructions)
        {
            Instruction = instruction;
            MicroInstructions = microInstructions;
        }

        public MicroFunction(string instruction, MicroInstruction microInstruction)
        {
            Instruction = instruction;
            MicroInstructions = new List<MicroInstruction> {microInstruction};
        }
    }
}