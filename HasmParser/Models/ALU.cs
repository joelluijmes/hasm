using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;
using NLog;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace hasm.Parsing.Models
{
    public sealed class ALU
    {
        private static readonly Dictionary<string, AluOperation> _operations = new Dictionary<string, AluOperation>
        {
            ["-"] = AluOperation.Minus,
            ["+"] = AluOperation.Plus,
            ["&"] = AluOperation.And,
            ["|"] = AluOperation.Or,
            ["^"] = AluOperation.Xor
        };

        public string Target { get; set; }
        public string Left { get; set; }
        public string Right { get; set; }
        public bool Carry { get; set; }
        public bool StackPointer { get; set; }
        public string Shift { get; set; }
        public AluOperation Operation { get; set; }

        public static ALU Parse(Node aluNode)
        {
            var alu = new ALU
            {
                Target = aluNode.FirstValueByNameOrDefault<string>("target"),
                Left = aluNode.FirstValueByNameOrDefault<string>("left"),
                Right = aluNode.FirstValueByNameOrDefault<string>("right"),
                Carry = aluNode.FirstValueByNameOrDefault<string>("carry") != null,
                StackPointer = aluNode.FirstValueByNameOrDefault<string>("SP") != null,
                Shift = aluNode.FirstValueByNameOrDefault<string>("shift")
            };

            var op = aluNode.FirstValueByNameOrDefault<string>("op");
            if (op != null)
                alu.Operation = _operations[op];
            
            return alu;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(Target))
                builder.Append($"{Target}=");
            if (StackPointer)
                builder.Append("SP=");
            if (!string.IsNullOrEmpty(Left))
                builder.Append(Left);

            if (Operation == AluOperation.Clear)
                return builder.ToString();

            var sign = _operations.FirstOrDefault(f => f.Value == Operation).Key;
            if (!string.IsNullOrEmpty(sign))
                builder.Append(sign);

            builder.Append(Right);

            if (Carry && !string.IsNullOrEmpty(sign))
                builder.Append($"{sign}C");

            if (!string.IsNullOrEmpty(Shift))
                builder.Append($"{Shift}1");

            return builder.ToString();
        }
    }
}