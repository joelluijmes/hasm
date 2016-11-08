using System;
using System.Collections.Generic;
using System.Linq;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using NLog;

namespace hasm
{
    internal sealed class MicroAssembler
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IList<MicroFunction> _microFunctions;

        public MicroAssembler(IList<MicroFunction> microFunctions)
        {
            _microFunctions = microFunctions;
        }

        public void Generate()
        {
            var list = new List<MicroFunction>();
            foreach (var microFunction in _microFunctions)
            {
                var operands = HasmGrammar.GetOperands(microFunction.Instruction)
                    .Select(type => PermuteOperands(type).Select(operand => new KeyValuePair<string, string>(type, operand)));
                var operandPermutations = operands.CartesianProduct();

                foreach (var permutation in operandPermutations)
                {
                    var function = microFunction.Clone();

                    foreach (var operand in permutation)
                    {
                        function.Instruction = function.Instruction.Replace(operand.Key, operand.Value);

                        foreach (var instruction in function.MicroInstructions)
                        {
                            var alu = instruction.ALU;
                            if (alu == null)
                                continue;

                            if (!string.IsNullOrEmpty(alu.Left))
                                alu.Left = alu.Left.Replace(operand.Key, operand.Value);
                            if (!string.IsNullOrEmpty(alu.Right))
                                alu.Right = alu.Right.Replace(operand.Key, operand.Value);
                            if (!string.IsNullOrEmpty(alu.Target))
                                alu.Target = alu.Target.Replace(operand.Key, operand.Value);
                        }
                    }

                    list.Add(function);
                }
            }

            var c = list.Sum(s => s.MicroInstructions.Count);
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
            {
                var splitted = operand.Split(new[] {' ', '+'}, StringSplitOptions.RemoveEmptyEntries);
                var operands = splitted.Select(PermuteOperands);

                return operands.CartesianProduct().Select(x => x.Aggregate((current, y) => $"{current}+{y}"));
            }
            default:
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}
