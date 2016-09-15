using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace MicParser.OpCode
{
    public struct MicroInstruction
    {
        public string Label { get; }
        public MicroOpCode OpCode { get; }
        public string Branch { get; }

        public MicroInstruction(string label, MicroOpCode opCode, string branch)
        {
            Label = label;
            OpCode = opCode;
            Branch = branch;
        }

        public static MicroInstruction FromNode(Node statement)
        {
            var labelNode = Evaluator.FirstNodeByName(statement, "Label");
            var label = labelNode != null ? Evaluator.FirstValue<string>(labelNode) : "";

            var aluNode = Evaluator.FirstNodeByName(statement, "ALU");
            var alu = aluNode != null ? Evaluator.FirstValue<long>(aluNode) : 0L;

            var memoryNode = Evaluator.FirstNodeByName(statement, "Memory");
            var memory = memoryNode != null ? Evaluator.FirstValue<long>(memoryNode) : 0L;

            var branchNode = Evaluator.FirstNodeByName(statement, "Branch");
            var branch = branchNode != null ? "goto " + Evaluator.FirstValue<string>(branchNode) : "";

            var opcode = new MicroOpCode {Value = alu | memory};
            return new MicroInstruction(label, opcode, branch);
        }

        public override string ToString() => $"{Label}:\t{OpCode.Value:X9}; {Branch}";
    }
}