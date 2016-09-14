using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace MicParser.OpCode
{
    public struct MicroInstruction
    {
        public string Label { get; }
        public MicroOpCode OpCode { get; }

        public MicroInstruction(string label, MicroOpCode opCode)
        {
            Label = label;
            OpCode = opCode;
        }

        public static MicroInstruction FromNode(Node statement) => new MicroInstruction(
            Evaluator.FirstValue<string>(statement),
            new MicroOpCode {Value = Evaluator.FirstValue<long>(statement)});

        public override string ToString() => $"{Label}: {OpCode.Value:X9}";
    }
}