using System.Text.RegularExpressions;
using ParserLib;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace MicParser.OpCode
{
    public class MicroInstruction
    {
        public string Label { get; set; }
        public MicroOpCode OpCode { get; set; }
        public string Branch { get; set; }
        public int Address { get; set; } = -1;

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

            var opcode = new MicroOpCode { Value = alu | memory };

            var branchNode = statement.FindByName("Branch");
            if (branchNode != null)
            {
                var knownBranch = Evaluator.FirstValueNodeOrDefault<long>(branchNode);
                if (knownBranch != null)
                    opcode.NextAddress = (ushort) knownBranch.Value;
            }

            var branch = branchNode != null && opcode.NextAddress == 0 ? branchNode.Value<string>() : "";
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