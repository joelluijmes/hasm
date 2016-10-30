using System;
using System.Collections.Generic;
using System.Linq;
using hasm.Exceptions;
using hasm.Parsing;
using NLog;
using ParserLib.Evaluation;

namespace hasm
{
	internal sealed class Assembler
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IDictionary<string, int> _labelLookup;
		private readonly IList<Instruction> _listing;
		private readonly Instruction _nopInstruction;
		private readonly HasmParser _parser;

		public Assembler(HasmParser parser, IEnumerable<string> listing)
		{
			_parser = parser;
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

				var address = _labelLookup[operand];
				instruction.Input = $"{opcode} {address}";
			}

			instruction.Encoding = _parser.Encode(instruction.Input);
			instruction.Completed = true;
		}

		private void FirstPass(Instruction instruction, ref int address)
		{
			byte[] encoded;
			var completed = _parser.TryEncode(instruction.Input, out encoded);
			if (encoded == null)
				throw new NotImplementedException();

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

			var label = line == string.Empty ? string.Empty : HasmGrammar.ListingLabel.FirstValueOrDefault(line);
			if (!string.IsNullOrEmpty(label))
			{
				line = line.Substring(label.Length + 1).Trim();
				label = label.Trim();
			}

			var input = line == string.Empty ? string.Empty : HasmGrammar.ListingInstruction.FirstValueOrDefault(line);
			if (!string.IsNullOrEmpty(input))
				input = input.Trim();

			return new Instruction(label, input);
		}
	}
}