using System.Collections.Generic;
using System.Linq;
using hasm.Parsing;
using ParserLib.Evaluation;

namespace hasm
{
	internal sealed class Assembler
	{
		private readonly HasmParser _parser;
		private readonly IList<Instruction> _listing;

		public Assembler(HasmParser parser, IEnumerable<string> listing)
		{
			_parser = parser;
			_listing = listing.Select(ParseFromLine).ToList();
		}

		public byte[] Process()
		{
			for (var i = 0; i < _listing.Count; i++)
				_listing[i] = Encode(_listing[i]);

			return null;
		}

		private EncodedInstruction Encode(Instruction instruction)
		{
			var encoded = _parser.Encode(instruction.Input);
			return new EncodedInstruction(instruction, encoded);
		}

		private static Instruction ParseFromLine(string line)
		{
			line = line.Trim();

			var label = HasmGrammar.ListingLabel.FirstValueOrDefault(line);
			if (label != null)
			{
				line = line.Substring(label.Length + 1).Trim();
				label = label.Trim();
			}

			var input = HasmGrammar.ListingInstruction.FirstValueOrDefault(line);
			if (input != null)
			{
				line = line.Substring(input.Length).Trim();
				input = input.Trim();
			}

			var comment = HasmGrammar.ListingComment.FirstValueOrDefault(line);
			comment = comment?.Trim();

			return new Instruction(label, input, comment);
		}
	}
}