using System.Collections.Generic;
using System.Linq;

namespace hasm.Parsing.Models
{
    public sealed class MicroFunction
    {
        public string Instruction { get; set; }
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

        public bool Equals(MicroFunction other)
        {
            return string.Equals(Instruction, other.Instruction) && Equals(MicroInstructions, other.MicroInstructions);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as MicroFunction;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Instruction?.GetHashCode() ?? 0)*397) ^ (MicroInstructions?.GetHashCode() ?? 0);
            }
        }
    }
}