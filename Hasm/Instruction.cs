using hasm.Parsing;
using ParserLib.Evaluation;

namespace hasm
{
	internal sealed class Instruction
	{
		public string Label { get; set; }
		public int Address { get; set; }
		public string Input { get; set; }
		public string Comment { get; set; }

		public static Instruction ParseFromLine(HasmParser hasmParser, string line)
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

			return new Instruction
			{
				Label = label,
				Input = input,
				Comment = comment
			};
		}
		
	}
}