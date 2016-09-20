using System.Text.RegularExpressions;
using MicParser.OpCode;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace MicParser
{
    public class MicroInstruction
    {
        public MicroInstruction(string label, MicroOpCode opCode, string branch)
        {
            Label = label;
            OpCode = opCode;
            Branch = branch;
        }

        public string Label { get; set; }
        public MicroOpCode OpCode { get; set; }
        public string Branch { get; set; }
        public int Address { get; set; } = -1;

        public static MicroInstruction FromNode(Node statement)
        {
            // TODO: Cleanup
            var labelNode = statement.FirstNodeByNameOrDefault("Label");
            var label = labelNode != null ? labelNode.FirstValue<string>() : "";

            var aluNode = statement.FirstNodeByNameOrDefault("ALU");
            var alu = aluNode?.FirstValue<long>() ?? 0L;

            var memoryNode = statement.FirstNodeByNameOrDefault("Memory");
            var memory = memoryNode?.FirstValue<long>() ?? 0L;

            var opcode = new MicroOpCode {Value = alu | memory};

            var branchNode = statement.FirstNodeByNameOrDefault("Branch");
            var knownBranch = branchNode?.FirstValueNodeOrDefault<long>();
            if (knownBranch != null)
                opcode.NextAddress = (ushort) knownBranch.Value;

            var branch = (branchNode != null) && (opcode.NextAddress == 0) ? branchNode.FirstValue<string>() : "";
            if (label == branch)
                label = "";

            return new MicroInstruction(label, opcode, branch);
        }

        public override string ToString()
        {
            var address = Address == -1 ? "XXX" : $"{Address:X3}";
            var branch = string.IsNullOrEmpty(Branch) ? "" : "goto " + Branch;

            return $"{address}  {Label}:\t{Regex.Replace($"{OpCode.Value:X9}", ".{3}", "$0 ")} {branch}";
        }
    }
}