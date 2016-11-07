using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;

namespace hasm
{
    internal sealed class MicoAssembler
    {
        private static readonly Dictionary<MicroFunction, IList<MicroFunction>> _permutations = new Dictionary<MicroFunction, IList<MicroFunction>>();
        private readonly IList<MicroFunction> _microFunctions;

        public MicoAssembler(IList<MicroFunction> microFunctions)
        {
            _microFunctions = microFunctions;
        }

        public void Generate()
        {
            foreach (var function in _microFunctions)
            {
                var p = Permute(function).ToList();
            }
        }

        private static IEnumerable<MicroFunction> Permute(MicroFunction microFunction)
        {
            var operands = HasmGrammar.GetOperands(microFunction.Instruction);
            var operandPermutations = operands.ToDictionary(o => o, PermuteOperands);

            operandPermutations.SelectMany(s => s.)

            var permutations = new List<MicroFunction>();
            foreach (var permutation in operandPermutations)
            {
                foreach (var x in permutation.Value)
                {
                                
                }    
            }

            return null;
        }
        
        private static IEnumerable<MicroInstruction> Permute(MicroInstruction microInstruction)
        {
            var alu = microInstruction.ALU;
            Func<string, string, string, MicroInstruction> creator =
                (target, left, right) =>
                {
                    var permutedAlu = alu.Clone();
                    permutedAlu.Target = target;
                    permutedAlu.Left = left;
                    permutedAlu.Right = right;

                    var permutedInstruction = microInstruction.Clone();
                    if (!string.IsNullOrEmpty(alu.Target))
                        permutedInstruction.Label = permutedInstruction.Label.Replace(alu.Target, target);
                    if (!string.IsNullOrEmpty(alu.Left))
                        permutedInstruction.Label = permutedInstruction.Label.Replace(alu.Left, left);
                    if (!string.IsNullOrEmpty(alu.Right))
                        permutedInstruction.Label = permutedInstruction.Label.Replace(alu.Right, right);

                    permutedInstruction.ALU = permutedAlu;

                    return permutedInstruction;
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
