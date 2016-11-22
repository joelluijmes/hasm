using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using hasm.Parsing;
using hasm.Parsing.DependencyInjection;
using hasm.Parsing.Encoding;
using hasm.Parsing.Export;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using hasm.Parsing.Parsers.Sheet;
using NLog;

namespace hasm
{
    internal sealed class MicroAssembler
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly HasmGrammar _hasmGrammar = KernelFactory.Resolve<HasmGrammar>();
        private static readonly HasmEncoder _sheetSheetProvider = new HasmEncoder(_hasmGrammar);
        
        public IEnumerable<IAssembled> Assemble(IList<MicroFunction> microFunctions)
        {
            _logger.Info($"Assembling {microFunctions.Count} micro-functions..");

            var sw = Stopwatch.StartNew();

            var distinct = DistinctInstructions(microFunctions).ToList();
            var assembled = AssembleFunctions(distinct);
            var microInstructions = assembled
                .GroupBy(i => i.Address)
                .Select(i => i.First())
                .OrderBy(i => i.Address)
                .ToList();

            var lastNop = MicroInstruction.NOP;
            lastNop.Location = 0xFFFF;
            //microInstructions.Add(new AssembledInstruction(lastNop));

            sw.Stop();
            _logger.Info($"Assembled {microInstructions.Count} micro-instructions in {sw.Elapsed}");
            
           return microInstructions;
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
            var encoded = _sheetSheetProvider.Encode(function.Instruction);

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

        private class AssembledInstruction : IAssembled
        {
            public AssembledInstruction(MicroInstruction instruction)
            {
                Instruction = instruction;
                Address = instruction.Location;
                Assembled = PropertyEncoder.Encode(instruction);
            }

            public long Assembled { get; } //=> PropertyEncoder.Encode(Instruction);
            public int Address { get; }
            public int Count => 37;
            public MicroInstruction Instruction { get; }

            public override string ToString() => Instruction.ToString();
        }
    }
}

