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

		public Assembler(HasmParser parser, IEnumerable<string> listing)
		{
			_parser = parser;
			_listing = listing
				.Where(l => !string.IsNullOrWhiteSpace(l))
				.Select(ParseFromLine)
				.Where(l => !string.IsNullOrEmpty(l.Input)) // only those containing an instruction
				.ToList();
		}

		public byte[] Process()
		{
			for (var i = 0; i < _listing.Count; i++)
				_listing[i] = TryEncode(_listing[i]);

			return null;
		}

		private Instruction TryEncode(Instruction instruction)
		{
			if (instruction is EncodedInstruction)
				return instruction;

			byte[] encoded;
			return _parser.TryEncode(instruction.Input, out encoded)
				? new EncodedInstruction(instruction, encoded)
				: instruction;
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