using MicParser.OpCode;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace MicParser
{
    public struct MicroInstruction
    {
        public string Label { get; }
        public MicroOpCode OpCode { get; }
        public string Next { get; }


        public MicroInstruction(string label, MicroOpCode opCode, string next)
        {
            Label = label;
            OpCode = opCode;
            Next = next;
        }

        public static MicroInstruction FromNode(Node statement) => new MicroInstruction(
            Evaluator.FirstValue<string>(statement),
            new MicroOpCode {Value = Evaluator.FirstValue<long>(statement)}, "");

        public override string ToString() => $"{Label}: {OpCode.Value:X9}";
    }
}