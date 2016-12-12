using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly HasmEncoder _encoder;
        private readonly IDictionary<string, string> _labelLookup;
        private readonly IAssembledInstruction _nopAssembledInstruction;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="HasmAssembler" /> class.
        /// </summary>
        /// <param name="encoder">Encoder for encoding the instructions.</param>
        public HasmAssembler(HasmEncoder encoder)
        {
            _encoder = encoder;
            _labelLookup = new Dictionary<string, string>();
            
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
                for (var j = instruction.Address - instruction.Count/8; j > previous.Address; --j)
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
            
            IAssembledInstruction assembled = new AssembledInstruction(encoded, 0, completed);
            return new ParsedInstructionModel(line, assembled);
        }

        private ParsedInstructionModel FirstPass(Line line, ref int address)
        {
            if (line.IsDirective)
            {
                var directive = ParseDirective(line, ref address);
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
                if (address%2 != 0)
                {
                    ++address;
                    _logger.Debug("Address not aligned on 16 bit");
                }

                if (_labelLookup.ContainsKey(line.Label))
                    throw new AssemblerException("Label was already defined in listing");

                _labelLookup[line.Label] = address.ToString();
                _logger.Debug($"Fixed '{line.Instruction}' at {address}");
            }

            IAssembledInstruction assembled = new AssembledInstruction(encoded, address, completed);
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

            var operands = HasmGrammar.GetOperands(instruction.Input.Instruction);

            var input = new StringBuilder(opcode);

            for (var i = 0; i < operands.Length; i++)
            {
                var operand = operands[i];

                string address;
                input.Append(' ');
                input.Append(_labelLookup.TryGetValue(operand, out address)
                    ? address
                    : operand);

                if (i < operands.Length - 1)
                    input.Append(',');
            }

            var encoded = _encoder.Encode(input.ToString());
            var assembled = new AssembledInstruction(encoded, instruction.Assembled.First().Address, true);
            return new ParsedInstructionModel(instruction.Input, assembled);
        }

        private IList<IAssembledInstruction> ParseDirective(Line line, ref int address)
        {
            switch (line.Directive)
            {
            case DirectiveTypes.EQU:
            {
                var label = HasmGrammar.DirectiveEqual.FirstValueByName<string>(line.Operands, "label");
                var value = HasmGrammar.DirectiveEqual.FirstValueByName<int>(line.Operands, "value");

                _labelLookup[label] = value.ToString();
                return null;
            }

            case DirectiveTypes.DEF:
            {
                var label = HasmGrammar.DirectiveDefine.FirstValueByName<string>(line.Operands, "label");
                var value = HasmGrammar.DirectiveDefine.FirstValueByName<string>(line.Operands, "text");

                _labelLookup[label] = value;
                return null;
            }

            case DirectiveTypes.DB:
            {
                var tree = HasmGrammar.DefineByte.ParseTree(line.Operands);

                var list = new List<IAssembledInstruction>();
                foreach (var node in tree.Descendents(n => n.IsValueNode<byte>()))
                {
                    var value = node.FirstValue<byte>();
                    list.Add(new DefinedByte(value, address));
                    address += 2;
                }

                return list;
            }


            }


            throw new NotImplementedException();
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

            throw new AssemblerException($"Invalid input, couldn't detect if it supposed to be instruction or a directive. Input: {line}");
        }
        
        private class DefinedByte : IAssembledInstruction
        {
            public DefinedByte(byte assembled, int address)
            {
                Assembled = assembled;
                Address = address;
            }

            public int Address { get; set; }
            public int Count => 8;
            public long Assembled { get; }
            public bool FullyAssembled => true;
        }

        private class AssembledInstruction : IAssembledInstruction
        {
            public AssembledInstruction(byte[] encoding, int address, bool fullyAssembled)
            {
                Assembled = ConvertToInt(encoding);
                Count = encoding.Length*8;
                Address = address;
                FullyAssembled = fullyAssembled;
            }

            public int Address { get; set; }
            public int Count { get; }
            public long Assembled { get; }
            public bool FullyAssembled { get; }

            private static long ConvertToInt(byte[] array)
            {
                if (array == null)
                    throw new ArgumentNullException(nameof(array));

                var result = 0;
                for (var i = 0; i < array.Length; i++)
                    result |= array[i] << (i*8);

                return result;
            }
        }

        private class ParsedInstructionModel
        {
            public ParsedInstructionModel(Line input, IAssembledInstruction assembled)
            {
                Assembled = new[] {assembled};
                Input = input;
            }

            public ParsedInstructionModel(Line input, IList<IAssembledInstruction> assembled)
            {
                Assembled = assembled;
                Input = input;
            }

            public Line Input { get; }
            public IList<IAssembledInstruction> Assembled { get; }
            public bool FullyAssembled => Assembled.All(i => i.FullyAssembled);
        }

        private interface IAssembledInstruction : IAssembled
        {
            bool FullyAssembled { get; }
        }
    }
}
