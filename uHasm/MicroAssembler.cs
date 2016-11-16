using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using hasm.Parsing.Encoding;
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
            _logger.Info($"Assembling {_microFunctions.Count} micro-functions..");

            var sw = Stopwatch.StartNew();

            FitMicroFunctions(_microFunctions);
            var microInstructions = _microFunctions
                .SelectMany(s => s.MicroInstructions)
                .GroupBy(s => s.Location)
                .Select(g => g.First())
                .OrderBy(i => i.Location)
                .ToList();

            //var lastNop = MicroInstruction.NOP;
            //lastNop.Location = 0xFFFF;
            //microInstructions.Add(lastNop);

            sw.Stop();
            _logger.Info($"Assembled {microInstructions.Count} micro-instructions in {sw.Elapsed}");

            WriteFile(microInstructions);
        }

        private static void WriteFile(List<MicroInstruction> microInstructions)
        {
            _logger.Info("Started writing to out.txt");

            var sw = Stopwatch.StartNew();
            using (var writer = new StreamWriter("out.txt"))
            {
                Action<MicroInstruction, StreamWriter> writeLine = (instr, writ) =>
                {
                    var value = PropertyEncoder.Encode(instr);
                    var encoded = Regex.Replace(Convert.ToString(value, 2).PadLeft(37, '0'), ".{4}", "$0 ");
                    var address = Regex.Replace(Convert.ToString(instr.Location, 2).PadLeft(16, '0'), ".{4}", "$0 ");

                    writ.WriteLine($"{address}: {encoded} {instr}");
                };

                MicroInstruction previous = null;
                foreach (var instruction in microInstructions)
                {
                    if (previous != null)
                    {
                        var diff = instruction.Location - previous.Location;
                        for (var i = 1; i < diff; ++i)
                        {
                            ++previous.Location;
                            writeLine(previous, writer);
                        }
                    }

                    writeLine(instruction, writer);
                    previous = instruction;
                }
            }

            sw.Stop();
            _logger.Info($"Completed in {sw.Elapsed}");
        }

        private static void FitMicroFunctions(IEnumerable<MicroFunction> microFunctions)
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
                    if (!instructions.TryGetValue(instruction, out cached))
                    {
                        instruction.Location = address++ << 6; // last bit doesn't count 
                        instruction.InternalInstruction = true;

                        instructions.Add(instruction, instruction);
                    }
                    else
                        function.MicroInstructions[i] = cached;

                    function.MicroInstructions[i - 1].NextInstruction = (instruction.Location & 0x7FFF) >> 6;
                }

                if (address > 512)
                    throw new NotImplementedException();
            }
        }
    }
}
