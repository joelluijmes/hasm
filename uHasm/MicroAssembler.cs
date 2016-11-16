using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        private readonly IList<MicroFunction> _microFunctions;

        public MicroAssembler(IList<MicroFunction> microFunctions)
        {
            _microFunctions = microFunctions;
        }

        public void Generate()
        {
            _logger.Info($"Assembling {_microFunctions.Count} micro-functions..");

            var sw = Stopwatch.StartNew();

            var distinct = DistinctInstructions(_microFunctions).ToList();
            var assembled = AssembleFunctions(distinct);
            var microInstructions = assembled
                .GroupBy(i => i.Location)
                .Select(i => i.First())
                .OrderBy(i => i.Location)
                .ToList();

            var lastNop = MicroInstruction.NOP;
            lastNop.Location = 0xFFFF;
            //microInstructions.Add(new AssembledInstruction(lastNop));

            sw.Stop();
            _logger.Info($"Assembled {microInstructions.Count} micro-instructions in {sw.Elapsed}");

            WriteFile(microInstructions);
        }

        private static void WriteFile(List<AssembledInstruction> microInstructions)
        {
            _logger.Info("Started writing to out.txt");

            var sw = Stopwatch.StartNew();
            using (var writer = new StreamWriter("out.txt"))
            {
                Action<AssembledInstruction, StreamWriter> writeLine = (instr, writ) =>
                {
                    var encoded = Regex.Replace(Convert.ToString(instr.Assembled, 2).PadLeft(37, '0'), ".{4}", "$0 ");
                    var address = Regex.Replace(Convert.ToString(instr.Location, 2).PadLeft(16, '0'), ".{4}", "$0 ");

                    writ.WriteLine($"{address}: {encoded} {instr.Instruction}");
                };

                AssembledInstruction previous = null;
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

        private static IList<MicroFunction> DistinctInstructions(IEnumerable<MicroFunction> microFunctions)
        {
            var cache = new Dictionary<MicroInstruction, MicroInstruction>();
            var list = new List<MicroFunction>();

            foreach (var microFunction in microFunctions)
            {
                if (microFunction.MicroInstructions.Count == 1)
                {
                    list.Add(microFunction);
                    continue;
                }

                var instructions = new List<MicroInstruction>(new[] {microFunction.MicroInstructions[0]});
                for (var i = 1; i < microFunction.MicroInstructions.Count; ++i)
                {
                    var instruction = microFunction.MicroInstructions[i];

                    MicroInstruction cached;
                    if (cache.TryGetValue(instruction, out cached))
                    {
                        instructions.Add(cached);
                        instruction = cached;
                    }
                    else
                    {
                        instructions.Add(instruction);
                        cache.Add(instruction, instruction);
                    }

                    instructions[i - 1].NextMicroInstruction = instruction;
                }

                list.Add(new MicroFunction(microFunction.Instruction, instructions));
            }

            return list;
        }

        private static IList<AssembledInstruction> AssembleFunctions(IEnumerable<MicroFunction> microFunctions)
        {
            var cache = new Dictionary<MicroInstruction, MicroInstruction>();
            var list = new List<MicroInstruction>();

            var address = 0;
            foreach (var function in microFunctions)
            {
                for (var i = function.MicroInstructions.Count - 1; i >= 1; --i)
                {
                    var instruction = function.MicroInstructions[i];
                    instruction.InternalInstruction = true;

                    if (cache.ContainsKey(instruction))
                        continue;

                    instruction.Location = ++address << 6;
                    cache.Add(instruction, instruction);
                    list.Add(instruction);
                }

                SetLocation(function);
                list.Add(function.MicroInstructions[0]);

                if (address == 512)
                    throw new NotImplementedException();
            }

            return list.Select(i => new AssembledInstruction(i)).ToList();
        }

        private static void SetLocation(MicroFunction function)
        {
            // first microinstruction/function address is the assembled (macro)instruction
            var encoded = _sheetParser.Encode(function.Instruction);

            switch (encoded.Length)
            {
            case 1:
                encoded = new byte[] {0x00, encoded[0]};
                break;
            case 3:
                encoded = new[] {encoded[1], encoded[2]};
                break;
            default:
                if (encoded.Length != 2)
                    throw new NotImplementedException();

                break;
            }

            var address = encoded.ConvertToInt();
            function.Location = address >> 1; // last bit doesn't count
        }

        private class AssembledInstruction
        {
            public AssembledInstruction(MicroInstruction instruction)
            {
                Instruction = instruction;
                Location = instruction.Location;
                Assembled = PropertyEncoder.Encode(instruction);
            }

            public long Assembled { get; } //=> PropertyEncoder.Encode(Instruction);
            public int Location { get; set; }
            public MicroInstruction Instruction { get; }

            public override string ToString() => $"{Convert.ToString(Location, 16)}: {Convert.ToString(Assembled, 16)} {Instruction}";
        }
    }
}

