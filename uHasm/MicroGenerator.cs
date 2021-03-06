﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using hasm.Parsing;
using hasm.Parsing.DependencyInjection;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using NLog;

namespace hasm
{
    internal static class MicroGenerator
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static IList<MicroFunction> GenerateMicroInstructions(IList<MicroFunction> microFunctions)
        {
            _logger.Info("Generating all possible instructions..");

            var sw = Stopwatch.StartNew();
#if DEBUG
            //microFunctions = new[] { microFunctions.ElementAt(40) };
#endif
            microFunctions = microFunctions.Take(10).ToList();


#if PARALLEL
            var concurrentQueue = new ConcurrentQueue<MicroFunction>();
            Parallel.ForEach(microFunctions, microFunction =>
            {
                var operands = HasmGrammar.GetOperands(microFunction.Instruction)
                                          .Select(type => PermuteOperands(type) // generate all permutations of operand
                                              .Select(operand => new Operand(type, operand)));

                foreach (var permutation in operands.CartesianProduct())
                {
                    var function = microFunction.Clone();
                    PermuteFunction(permutation, function);

                    concurrentQueue.Enqueue(function);
                }
            });

            var list = concurrentQueue.ToList();
#else
            var list = new List<MicroFunction>();
            foreach (var microFunction in microFunctions)
            {
                var operands = HasmGrammar.GetOperands(microFunction.Instruction)
                                          .Select(type => PermuteOperands(type) // generate all permutations of operand
                                              .Select(operand => new Operand(type, operand)));

                foreach (var permutation in operands.CartesianProduct())
                {
                    var function = microFunction.Clone();
                    PermuteFunction(permutation, function);

                    list.Add(function);
                }
            }
#endif

            sw.Stop();
            _logger.Info($"Generated {list.Count} micro-functions in {sw.Elapsed}");

            return list;
        }

        public static void PermuteFunction(IEnumerable<Operand> permutation, MicroFunction function)
        {
            var operands = permutation.SelectMany(SplitAggregated).ToArray();
            for (var i = 0; i < operands.Length; i++)
            {
                if (function.Instruction.Contains(operands[i].Type))
                    operands[i].ExternalOperand = true;

                foreach (var alu in function.MicroInstructions.Select(s => s.ALU))
                {
                    if (alu == null)
                        continue;

                    if (!string.IsNullOrEmpty(alu.Left) && alu.Left.Contains(operands[i].Type))
                    {
                        alu.Left = alu.Left.Replace(operands[i].Type, operands[i].Value);
                        alu.ExternalLeft = function.Instruction.Contains(operands[i].Type);
                    }

                    if (!string.IsNullOrEmpty(alu.Right) && alu.Right.Contains(operands[i].Type))
                    {
                        alu.Right = alu.Right.Replace(operands[i].Type, operands[i].Value);
                        alu.ExternalRight = function.Instruction.Contains(operands[i].Type);
                    }

                    if (!string.IsNullOrEmpty(alu.Target))
                        alu.Target = alu.Target.Replace(operands[i].Type, operands[i].Value);
                }

                function.Instruction = function.Instruction.Replace(operands[i].Type, operands[i].Value);
            }
        }
        
        private static IEnumerable<Operand> SplitAggregated(Operand keyValue)
        {
            var types = keyValue.Type.Split(new[] {' ', '+'}, StringSplitOptions.RemoveEmptyEntries);
            var values = keyValue.Value.Split(new[] {' ', '+'}, StringSplitOptions.RemoveEmptyEntries);

            if (types.Length != values.Length)
                throw new NotImplementedException();

            return types.Zip(values, (type, value) => new Operand(type, value));
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
        
        public struct Operand
        {
            public string Type { get; }
            public string Value { get; }
            public bool ExternalOperand { get; set; }

            public Operand(string type, string value)
            {
                Type = type;
                Value = value;
                ExternalOperand = false;
            }
        }
    }
}
