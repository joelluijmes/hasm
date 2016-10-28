using System;
using System.Collections.Generic;
using System.Linq;
using hasm.Parsing;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace hasm
{
	internal sealed class Assembler
	{
		private readonly HasmParser _parser;
		private readonly IList<Instruction> _listing;
		private readonly IDictionary<string, int> _labelLookup;

		public Assembler(HasmParser parser, IEnumerable<string> listing)
		{
			_parser = parser;
			_listing = listing
				.Where(l => !string.IsNullOrWhiteSpace(l))
				.Select(ParseFromLine)
				.Where(l => !string.IsNullOrEmpty(l.Input)) // only those containing an instruction
				.ToList();

			_labelLookup = new Dictionary<string, int>();
		}

		public byte[] Process()
		{
			for (var i = 0; i < _listing.Count; i++)
				_listing[i] = TryEncode(_listing[i]);
			var encoded = _listing.Cast<EncodedInstruction>().ToList();

			var address = 0;
			foreach (var instruction in encoded)
			{
				if (!string.IsNullOrEmpty(instruction.Label))
					_labelLookup[instruction.Label] = address;

				address += instruction.Encoded.Length;
			}

			for (var i = 0; i < _listing.Count; i++)
			{
				if (encoded[i].Completed)
					continue;

				_listing[i] = encoded[i] = Encode(_listing[i]);
			}

			var bytes = new List<byte>();
			foreach (var instruction in encoded)
				bytes.AddRange(instruction.Encoded);

			return bytes.ToArray();
		}

		private EncodedInstruction Encode(Instruction instruction)
		{
			var opcode = HasmGrammar.Opcode.FirstValue(instruction.Input);
			var operand = instruction.Input.Substring(opcode.Length + 1);	// skip the space
			if (!Grammar.Label.Match(operand))
				throw new NotImplementedException();

			var address = _labelLookup[operand];
			var input = $"{opcode} {address}";
			var encoded = _parser.Encode(input);

			return new EncodedInstruction(input, encoded);
		}

		private EncodedInstruction TryEncode(Instruction instruction)
		{
			var encodedInstruction = instruction as EncodedInstruction;
			if (encodedInstruction != null)
				return encodedInstruction;

			byte[] encoded;
			var completed = _parser.TryEncode(instruction.Input, out encoded);
			if (encoded == null)
				throw new NotImplementedException();

			return new EncodedInstruction(instruction, encoded, completed);
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
			{
				line = line.Substring(input.Length).Trim();
				input = input.Trim();
			}

			var comment = line == string.Empty ? string.Empty : HasmGrammar.ListingComment.FirstValueOrDefault(line);
			comment = comment?.Trim();

			return new Instruction(label, input, comment);
		}
	}
}