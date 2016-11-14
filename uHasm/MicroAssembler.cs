using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using hasm.Parsing;
using hasm.Parsing.Encoding;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using hasm.Parsing.Parsers.Sheet;
using NLog;

namespace hasm
{
    internal sealed class MicroAssembler
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly HasmSheetParser _sheetParser = new HasmSheetParser(new HasmGrammar());

        private readonly IList<MicroFunction> _microProgram;

        public MicroAssembler(IList<MicroFunction> microProgram)
        {
            _microProgram = microProgram;
        }

        public void Generate()
        {
            _logger.Info("Generating all possible instructions..");

            var sw = Stopwatch.StartNew();
            var nop = _microProgram.First(m => m.Instruction == "NOP").MicroInstructions[0];
#if DEBUG
            var program =   new[] { _microProgram.ElementAt(1) };
#else
            var program = _microProgram;
#endif
            var microFunctions = GenerateMicroInstructions(program);
            sw.Stop();

            _logger.Info($"Generated {microFunctions.Count} micro-functions in {sw.Elapsed}");
            _logger.Info("Encoding all possible instructions..");

            sw.Restart();

            FitMicroFunctions(microFunctions);
            var microInstructions = microFunctions
                .SelectMany(s => s.MicroInstructions)
                .GroupBy(s => s.Location)
                .Select(g => g.First())
                .OrderBy(i => i.Location)
                .ToList();

            var count = microInstructions.Count();

            sw.Stop();
            _logger.Info($"Encoded {microFunctions.Count} micro-functions (in total {count} micro-instructions) in {sw.Elapsed}");

            _logger.Info("Started writing to out.txt");
            sw.Restart();
            using (var writer = new StreamWriter("out.txt"))
            {
                Action<MicroInstruction, StreamWriter> writeLine = (instr, writ) =>
                {
                    var value = PropertyEncoder.Encode(instr);
                    var encoded = Regex.Replace(Convert.ToString(value, 2).PadLeft(37, '0'), ".{4}", "$0 ");
                    var address = Regex.Replace(Convert.ToString(instr.Location, 2).PadLeft(16, '0'), ".{4}", "$0 ");

                    writ.WriteLine($"{address}: {encoded} {instr}");
                };

                //var nopCopy = nop.Clone();
                //nopCopy.Location = 0xFFFF;
                //microInstructions.Add(nopCopy);

                MicroInstruction previous = null;
                foreach (var instruction in microInstructions)
                {
                    if (previous != null)
                    {
                        for (var i = 1; i < instruction.Location - previous.Location; ++i)
                        {
                            //if (previous.ALU.ExternalImmediate && instruction.ALU.ExternalImmediate)
                            //{
                                previous.Location = ++previous.Location;
                                writeLine(previous, writer);
                            //}
                            //else
                            //{
                            //    nop.Location = previous.Location + i;
                            //    writeLine(nop, writer);
                            //}
                        }
                    }

                    writeLine(instruction, writer);
                    previous = instruction;
                }
            }

            sw.Stop();
            _logger.Info($"Completed in {sw.Elapsed}");

            foreach (var function in microFunctions)
            {
                _logger.Info(function);

                foreach (var instruction in function.MicroInstructions)
                {
                    var value = PropertyEncoder.Encode(instruction);
                    var encoded = Regex.Replace(Convert.ToString(value, 2).PadLeft(37, '0'), ".{4}", "$0 ");
                    var address = Regex.Replace(Convert.ToString(instruction.Location, 2).PadLeft(16, '0'), ".{4}", "$0 ");

                    _logger.Info(address +
                                  $" {instruction.ToString().PadRight(25)}" +
                                  $" {encoded}" +
                                  $" {(instruction.LastInstruction ? "" : Convert.ToString(instruction.NextInstruction, 16).PadLeft(3, '0') + "h")}");

                    ++count;
                }

                if (count > 400)
                    break;
            }
        }

        private void FitMicroFunctions(IList<MicroFunction> microFunctions)
        {
            var instructions = new Dictionary<MicroInstruction, MicroInstruction>();

            var address = 0;
            foreach (var function in microFunctions)
            {
                if (function.MicroInstructions.Count == 1)
                    continue;

                for (var i = 1; i < function.MicroInstructions.Count; ++i)
                {
                    var instruction = function.MicroInstructions[i];

                    MicroInstruction cached;
                    instructions.TryGetValue(instruction, out cached);

                    if (cached != null) // reuse a previous created microinstruction
                        function.MicroInstructions[i] = cached;
                    else
                    {
                        function.MicroInstructions[i].Location = (address++ << 6); // last bit doesn't count 
                        function.MicroInstructions[i].InternalInstruction = true;
                    }

                    function.MicroInstructions[i - 1].NextInstruction = function.MicroInstructions[i].Location >> 6;

                    if (cached == null)
                        instructions.Add(instruction, instruction);
                }

                if (address > 512)
                    throw new NotImplementedException();
            }
        }

        private static IList<MicroFunction> GenerateMicroInstructions(IEnumerable<MicroFunction> microFunctions)
        {
#if PARALLEL
            var concurrentQueue = new ConcurrentQueue<MicroFunction>();
            Parallel.ForEach(microFunctions, microFunction =>
            {
                var operands = HasmGrammar.GetOperands(microFunction.Instruction)
                                          .Select(type => PermuteOperands(type) // generate all permutations of operand
                                              .Select(operand => new KeyValuePair<string, string>(type, operand))); // put it in a key:value

                foreach (var permutation in operands.CartesianProduct())
                {
                    var function = microFunction.Clone();
                    PermuteFunction(permutation, function);

                    concurrentQueue.Enqueue(function);
                }
            });

            return concurrentQueue.ToList();
#else
            var list = new List<MicroFunction>();
            foreach (var microFunction in microFunctions)
            { 
                var operands = HasmGrammar.GetOperands(microFunction.Instruction)
                                          .Select(type => PermuteOperands(type) // generate all permutations of operand
                                              .Select(operand => new KeyValuePair<string, string>(type, operand))); // put it in a key:value

                foreach (var permutation in operands.CartesianProduct())
                {
                    var function = microFunction.Clone();
                    PermuteFunction(permutation, function);

                    list.Add(function);
                }
            }

            return list;
#endif
        }

        private static void PermuteFunction(IEnumerable<KeyValuePair<string, string>> permutation, MicroFunction function)
        {
            var externalImmediate = false;
            foreach (var operand in permutation.SelectMany(SplitAggregated))
            {
                if (operand.Key.StartsWith("IMM"))
                    externalImmediate = true;

                function.Instruction = function.Instruction.Replace(operand.Key, operand.Value);

                foreach (var alu in function.MicroInstructions.Select(s => s.ALU))
                {
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
            
            // first microinstruction/function address is the assembled (macro)instruction
            var encoded = _sheetParser.Encode(function.Instruction);

            if (encoded.Length == 1)
                encoded = new byte[] { 0x00, encoded[0]};
            else if (encoded.Length == 3)
                encoded = new[] {encoded[1], encoded[2]};
            else if (encoded.Length != 2)
                throw new NotImplementedException();

            var address = ConvertToInt(encoded);
            function.MicroInstructions[0].Location = address >> 1; // last bit doesn't count

            if (externalImmediate)
                function.MicroInstructions[0].ALU.SetExternalImmediate();
        }

        private static IEnumerable<KeyValuePair<string, string>> SplitAggregated(KeyValuePair<string, string> keyValue)
        {
            var keys = keyValue.Key.Split(new[] {' ', '+'}, StringSplitOptions.RemoveEmptyEntries);
            var values = keyValue.Value.Split(new[] {' ', '+'}, StringSplitOptions.RemoveEmptyEntries);

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

        private static int ConvertToInt(byte[] array)
        {
            var result = 0;
            for (var i = 0; i < array.Length; i++)
                result |= array[i] << (i*8);

            return result;
        }
    }
}
