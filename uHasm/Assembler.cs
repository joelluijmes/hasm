using System;
using System.Collections.Generic;
using System.Linq;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;

namespace hasm
{
    internal sealed class Assembler
    {
        private readonly IList<MicroInstruction> _microInstructions;

        public Assembler(IList<MicroInstruction> microInstructions)
        {
            _microInstructions = microInstructions;
        }

        public void Generate()
        {
            var instruction = _microInstructions.Skip(40).First();
            var permutes = Permute(instruction.ALU).ToList();
        }

        private static IEnumerable<ALU> Permute(ALU alu)
        {
            if (alu == null)
                return Enumerable.Empty<ALU>();

            Func<string, string, string, ALU> creator =
                (target, left, right) => new ALU
                {
                    Target = target,
                    Left = left,
                    Right = right,
                    Operation = alu.Operation,
                    Shift = alu.Shift,
                    Carry = alu.Carry,
                    StackPointer = alu.StackPointer
                };

            var targets = PermuteOperands(alu.Target);
            if (alu.Target == alu.Left && string.IsNullOrWhiteSpace(alu.Right))
            {
                return from t in targets
                       select creator(t, t, null);
            }

            var lefts = PermuteOperands(alu.Left);
            var rights = PermuteOperands(alu.Right);

            return from t in targets
                   from l in lefts
                   from r in rights
                   select creator(t, l, r);
        }

        private static IEnumerable<string> PermuteOperands(string operand)
        {
            if (string.IsNullOrEmpty(operand))
                return new string[] {null}; // return array[1] because we still want to generate the rest of the permutations

            var encoding = HasmGrammar.FindOperandParser(operand)?.OperandEncoding;
            if (encoding == null)
                return new[] {operand}; // operandParser returned null -> must be a 'static' operand

            switch (encoding.Type)
            {
            case OperandEncodingType.KeyValue:
                return encoding.Pairs.Select(p => p.Key);
            case OperandEncodingType.Range:
                return Enumerable.Range(encoding.Minimum, encoding.Maximum - encoding.Minimum + 1).Select(i => i.ToString());
            case OperandEncodingType.Aggregation:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}
