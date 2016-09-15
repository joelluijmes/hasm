using ParserLib;
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
            var labelNode = statement.FindByName("Label");
            var label = labelNode != null ? labelNode.Value<string>() : "";

            var aluNode = statement.FindByName("ALU");
            var alu = aluNode?.Value<long>() ?? 0L;

            var memoryNode = statement.FindByName("Memory");
            var memory = memoryNode?.Value<long>() ?? 0L;

            var branchNode = statement.FindByName("Branch");
            var branch = branchNode != null ? "goto " + branchNode.Value<string>() : "";

            var opcode = new MicroOpCode {Value = alu | memory};
            return new MicroInstruction(label, opcode, branch);
        }

        public override string ToString() => $"{Label}:\t{OpCode.Value:X9}; {Branch}";
    }
}