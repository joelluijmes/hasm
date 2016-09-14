using ParserLib.Evaluation;
using ParserLib.Parsing;

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

        public static MicroInstruction FromNode(Node statement) => new MicroInstruction(
            Evaluator.FirstValue<string>(statement),
            Evaluator.FirstValue<long>(statement));

        public override string ToString() => $"{Label}: {Instruction:X9}";
    }
}