using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Text;
using hasm.Assembler;
using hasm.Assembler.Directives;
using hasm.Exceptions;
using hasm.Parsing.Encoding;
using hasm.Parsing.Export;
using hasm.Parsing.Grammars;
using NLog;
using ParserLib.Evaluation;
using ParserLib.Exceptions;
using ParserLib.Parsing;

namespace hasm
{
    /// <summary>
    ///     Used to assemble a listing into binary data.
    /// </summary>
    public sealed class HasmAssembler
    {
        public const int WORDSIZE = 2;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static IDictionary<DirectiveTypes, IDirective> _directiveParsers;

        private readonly HasmEncoder _encoder;
        private readonly IDictionary<string, string> _labelLookup;
        private readonly IAssemblingInstruction _nopAssembledInstruction;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HasmAssembler" /> class.
        /// </summary>
        /// <param name="encoder">Encoder for encoding the instructions.</param>
        public HasmAssembler(HasmEncoder encoder)
        {
            _encoder = encoder;
            _labelLookup = new Dictionary<string, string>();

            _directiveParsers = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IDirective).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => (IDirective) Activator.CreateInstance(t, _labelLookup))
                .ToDictionary(k => k.DirectiveType, v => v);
            
            var nop = ParseLine("nop");
            var instruction = FirstPass(nop);
            _nopAssembledInstruction = instruction.Assembled.Single();
        }

        /// <summary>
        ///     Assembles the listing given in the ctor.
        /// </summary>
        /// <returns>Assembled listing</returns>
        public IEnumerable<IAssembled> Process(IEnumerable<string> listing)
        {
            var preparsing = listing
                .Select(s => s.Trim())
                .Select(ParseLine)
                .Where(n => n != null)
                .ToArray();

            var address = 0;
            _logger.Info($"Started processing {preparsing.Length} instructions..");
            _labelLookup.Clear();

            var instructions = preparsing
                .Select(line => FirstPass(line, ref address))
                .Where(a => a != null)
                .ToList();

            _logger.Debug($"Performed first pass. Last instruction at: {address}");
            _logger.Debug($"Performing second pass..");

            instructions = instructions.Select(SecondPass).ToList();
            var assembled = instructions
                .SelectMany(m => m.Assembled)
                .OrderBy(m => m.Address)
                .ToList();

            for (var i = 1; i < assembled.Count; i++)
            {
                var instruction = assembled[i];
                var previous = assembled[i - 1];
                for (var j = previous.Address + previous.Bytes.Length; j < instruction.Address; ++j)
                {
                    _logger.Debug($"Address not sequential. Inserting a nop at {i}..");

                    _nopAssembledInstruction.Address = i;
                    assembled.Insert(i++, _nopAssembledInstruction);
                }
            }

            _logger.Info("Assembler done");
            return assembled;
        }

        private ParsedInstructionModel FirstPass(Line line)
        {
            byte[] encoded;
            var completed = _encoder.TryEncode(line.Instruction, out encoded);
            if (encoded == null)
                throw new AssemblerException($"Couldn't parse '{line.Instruction}'. Please check your grammar and/or input.");

            IAssemblingInstruction assembled = new AssembledInstruction(encoded, 0, completed);
            return new ParsedInstructionModel(line, assembled);
        }

        private ParsedInstructionModel FirstPass(Line line, ref int address)
        {
            if (line.IsDirective)
            {
                var directive = _directiveParsers[line.Directive].Parse(line, ref address);
                return directive == null
                    ? null
                    : new ParsedInstructionModel(line, directive);
            }

            byte[] encoded;
            var completed = _encoder.TryEncode(line.Instruction, out encoded);
            if (encoded == null)
                throw new AssemblerException($"Couldn't parse '{line.Instruction}'. Please check your grammar and/or input.");

            if (!string.IsNullOrEmpty(line.Label))
            {
                while (address% WORDSIZE != 0)
                {
                    ++address;
                    _logger.Debug($"Address not aligned on {WORDSIZE*8} bit");
                }

                if (_labelLookup.ContainsKey(line.Label))
                    throw new AssemblerException("Label was already defined in listing");

                var memoryAddress = address/WORDSIZE;
                _labelLookup[line.Label] = memoryAddress.ToString();
                _logger.Debug($"Fixed '{line.Instruction}' at {address:X4}h ({memoryAddress:X4}h in memory)");
            }

            IAssemblingInstruction assembled = new AssembledInstruction(encoded, address, completed);
            address += encoded.Length;
            return new ParsedInstructionModel(line, assembled);
        }

        private ParsedInstructionModel SecondPass(ParsedInstructionModel instruction)
        {
            if (instruction.FullyAssembled)
                return instruction;

            if (instruction.Assembled.Count > 1)
                throw new NotImplementedException();

            var opcode = HasmGrammar.Opcode.FirstValue(instruction.Input.Instruction);
            if (instruction.Input.Instruction == opcode)
                throw new NotImplementedException();

            var instructionEncoding = _encoder.FindInstructionEncoding(opcode);
            var operands = HasmGrammar.GetOperands(instruction.Input.Instruction);

            var input = new StringBuilder(opcode);

            for (var i = 0; i < operands.Length; i++)
            {
                var operand = operands[i];

                string address;
                input.Append(' ');
                if (_labelLookup.TryGetValue(operand, out address))
                {
                    var operandTypes = HasmGrammar.GetOperands(instructionEncoding.Grammar);
                    if (i >= operandTypes.Length || !operandTypes[i].StartsWith("IMM"))
                    {
                        input.Append(operand);
                    }
                    else
                    {
                        // signed operand means relative
                        if (operandTypes[i].EndsWith("s"))
                        {
                            var addressAsInteger = Grammar.Int32().FirstValue(address);
                            address = (addressAsInteger - instruction.Assembled[0].Address).ToString();
                        }

                        input.Append(address);
                    }
                }
                else input.Append(operand);

                if (i < operands.Length - 1)
                    input.Append(',');
            }

            var encoded = _encoder.Encode(input.ToString());
            var assembled = new AssembledInstruction(encoded, instruction.Assembled.First().Address, true);
            return new ParsedInstructionModel(instruction.Input, assembled);
        }
        
        private static Line ParseLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            var tree = HasmGrammar.Line.ParseTree(line);

            var label = tree.FirstValueByNameOrDefault<string>("label");
            var instruction = tree.FirstValueByNameOrDefault<string>("instruction");
            var strDirective = tree.FirstValueByNameOrDefault<string>("directive");
            var operands = tree.FirstValueByNameOrDefault<string>("operands");
            var comment = tree.FirstValueByNameOrDefault<string>("comment");

            if ((instruction == null) && (strDirective != null) && (operands != null))
            {
                try
                {
                    var directive = Grammar.EnumValue<DirectiveTypes>().FirstValue(strDirective);
                    return new Line(label, directive, operands, comment);
                }
                catch (ParserException e)
                {
                    throw new AssemblerException($"Invalid directive! Input: {line}", e);
                }
            }

            if ((instruction != null) && (strDirective == null) && (operands == null))
                return new Line(label, instruction, comment);

            if ((instruction == null) && (strDirective == null) && (operands == null))
                return null;

            throw new AssemblerException($"Invalid input, couldn't detect if it supposed to be instruction or a directive. Input: {line}");
        }

        

        private class AssembledInstruction : IAssemblingInstruction
        {
            public AssembledInstruction(byte[] encoding, int address, bool fullyAssembled)
            {
                Address = address;
                FullyAssembled = fullyAssembled;
                Bytes = encoding;
            }

            public int Address { get; set; }
            public bool FullyAssembled { get; }
            public byte[] Bytes { get; }
        }

        private class ParsedInstructionModel
        {
            public ParsedInstructionModel(Line input, IAssemblingInstruction assembled)
            {
                Assembled = new[] {assembled};
                Input = input;
            }

            public ParsedInstructionModel(Line input, IList<IAssemblingInstruction> assembled)
            {
                Assembled = assembled;
                Input = input;
            }

            public Line Input { get; }
            public IList<IAssemblingInstruction> Assembled { get; }
            public bool FullyAssembled => Assembled.All(i => i.FullyAssembled);
        }
    }
}
