using System.Collections.Generic;
using System.Linq;

namespace hasm.Parsing.Models
{
    public sealed class MicroFunction
    {
        public string Instruction { get; }
        public IList<MicroInstruction> MicroInstructions { get; }

        public MicroFunction(string instruction, IEnumerable<MicroInstruction> microInstructions)
        {
            Instruction = instruction;
            MicroInstructions = microInstructions.ToList();
        }

        public MicroFunction(string instruction, MicroInstruction microInstruction)
        {
            Instruction = instruction;
            MicroInstructions = new List<MicroInstruction> {microInstruction};
        }

        public MicroFunction Clone() => new MicroFunction(Instruction, MicroInstructions.Select(i => i.Clone()));
    }
}