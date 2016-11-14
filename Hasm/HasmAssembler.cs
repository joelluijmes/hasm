using System.Collections.Generic;
using System.Linq;
using hasm.Exceptions;
using hasm.Parsing.Grammars;
using hasm.Parsing.Parsers.Sheet;
using NLog;
using ParserLib.Evaluation;

namespace hasm
{
    /// <summary>
    ///     Used to assemble a listing into binary data.
    /// </summary>
    public sealed class HasmAssembler
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly HasmSheetParser _sheetParser = new HasmSheetParser(new HasmGrammar());
        private readonly IDictionary<string, int> _labelLookup;
        private readonly IList<Instruction> _listing;
        private readonly Instruction _nopInstruction;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HasmAssembler" /> class.
        /// </summary>
        /// <param name="listing">The listing to assemble.</param>
        public HasmAssembler(IEnumerable<string> listing)
        {
            _listing = listing
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(ParseFromLine)
                .Where(l => !string.IsNullOrEmpty(l.Input)) // only those containing an instruction
                .ToList();

            _labelLookup = new Dictionary<string, int>();

            _nopInstruction = new Instruction("nop");
            SecondPass(_nopInstruction);

            _logger.Info($"Instantiated Assembler with {_listing.Count} instructions");
        }

        /// <summary>
        ///     Assembles the listing given in the ctor.
        /// </summary>
        /// <returns>Assembled listing</returns>
        public IEnumerable<byte> Process()
        {
            _logger.Info("Started processing instructions..");
            var address = 0;
            foreach (var instruction in _listing)
                FirstPass(instruction, ref address);
            _logger.Debug($"Performed first pass. Last instruction at: {address}");

            // check if we didn't skip a address (due alignment)
            for (var i = 1; i < _listing.Count; i++)
            {
                var instruction = _listing[i];
                var previous = _listing[i - 1];
                for (var j = instruction.Address - instruction.Encoding.Length; j > previous.Address; --j)
                {
                    _logger.Info($"Address not sequential. Inserting a nop at {i}..");
                    _listing.Insert(i++, _nopInstruction);
                }
            }

            _logger.Debug($"Performing second pass..");
            foreach (var instruction in _listing.Where(l => !l.Completed))
                SecondPass(instruction);

            _logger.Info("Assembler done");
            return _listing.SelectMany(l => l.Encoding);
        }

        private void SecondPass(Instruction instruction)
        {
            var opcode = HasmGrammar.Opcode.FirstValue(instruction.Input);
            if (instruction.Input != opcode)
            {
                var operand = instruction.Input.Substring(opcode.Length + 1); // skip the space

                int address;
                if (!_labelLookup.TryGetValue(operand, out address))
                    throw new AssemblerException($"Couldn't find definition for '{operand}'. Expected a label.");

                instruction.Input = $"{opcode} {address}";
            }

            instruction.Encoding = _sheetParser.Encode(instruction.Input);
            instruction.Completed = true;
        }

        private void FirstPass(Instruction instruction, ref int address)
        {
            byte[] encoded;
            var completed = _sheetParser.TryEncode(instruction.Input, out encoded);
            if (encoded == null)
                throw new AssemblerException($"Couldn't parse '{instruction.Input}'. Please check your grammar and/or input.");

            instruction.Encoding = encoded;
            instruction.Completed = completed;

            _logger.Debug($"Instruction {instruction.Input} is fully assembled: {instruction.Completed} ({instruction.Encoding.Length*8} - bits)");
            CheckLabel(instruction, ref address);
        }

        private void CheckLabel(Instruction instruction, ref int address)
        {
            if (!string.IsNullOrEmpty(instruction.Label))
            {
                if (address%2 != 0)
                {
                    ++address;
                    _logger.Debug("Address not aligned on 16 bit");
                }

                if (_labelLookup.ContainsKey(instruction.Label))
                    throw new AssemblerException("Label was already defined in listing");

                _labelLookup[instruction.Label] = address;
                _logger.Debug($"Fixed {instruction.Input} at {address}");
            }

            address += instruction.Encoding.Length;
            instruction.Address = address;
        }

        private static Instruction ParseFromLine(string line)
        {
            line = line.Trim();
            if (string.IsNullOrEmpty(line))
                return null;

            var startColon = line.IndexOf(':');
            var startSemicolon = line.IndexOf(';');
            string label;
            if ((startColon != -1) && ((startSemicolon == -1) || (startColon < startSemicolon)))
            {
                label = HasmGrammar.ListingLabel.FirstValueOrDefault(line);
                if (string.IsNullOrEmpty(label))
                    throw new AssemblerException($"Invalid label name in {line}.");

                line = line.Substring(label.Length + 1).Trim();
                label = label.Trim();
            }
            else
                label = null;

            var input = line == string.Empty
                ? string.Empty
                : HasmGrammar.ListingInstruction.FirstValueOrDefault(line);

            if (!string.IsNullOrEmpty(input))
                input = input.Trim();

            return new Instruction(label, input);
        }
    }
}