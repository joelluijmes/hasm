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
            var microFunc = _microFunctions.Skip(60).Take(1);
            var list = GenerateMicroInstructions(_microFunctions);

            foreach (var function in list)
                foreach (var instruction in function.MicroInstructions)
                    instruction.Encode();
        }

        private static IEnumerable<MicroFunction> GenerateMicroInstructions(IEnumerable<MicroFunction> microFunctions)
        {
            foreach (var microFunction in microFunctions)
            {
                var operands = HasmGrammar.GetOperands(microFunction.Instruction)
                                          .Select(type => PermuteOperands(type) // generate all permutations of operand
                                                      .Select(operand => new KeyValuePair<string, string>(type, operand))); // put it in a key:value

                foreach (var permutation in operands.CartesianProduct())
                {
                    var function = microFunction.Clone();
                    PermuteFunction(permutation, function);

                    yield return function;
                }
            }
        }

        private static void PermuteFunction(IEnumerable<KeyValuePair<string, string>> permutation, MicroFunction function)
        {
            foreach (var operand in permutation.SelectMany(SplitAggregated))
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
        }

        private static IEnumerable<KeyValuePair<string, string>> SplitAggregated(KeyValuePair<string, string> keyValue)
        {
            var keys = keyValue.Key.Split(new[] { ' ', '+' }, StringSplitOptions.RemoveEmptyEntries);
            var values = keyValue.Value.Split(new[] { ' ', '+' }, StringSplitOptions.RemoveEmptyEntries);

            if (keys.Length != values.Length)
                throw new NotImplementedException();

            return keys.Zip(values, (key, value) => new KeyValuePair<string, string>(key, value));
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
                return Enumerable.Range(encoding.Minimum, encoding.Maximum - encoding.Minimum).Select(i => i.ToString());
            case OperandEncodingType.Aggregation:
            {
                var splitted = operand.Split(new[] {' ', '+'}, StringSplitOptions.RemoveEmptyEntries);
                var operands = splitted.Select(PermuteOperands);

                return operands.CartesianProduct().Select(x => x.Aggregate((current, y) => $"{current} {y}"));
            }
            default:
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}
