using System.Collections.Generic;
using System.Linq;
using hasm.Parsing;

namespace hasm
{
	internal sealed class Assembler
	{
		private readonly IList<Instruction> _listing;

		public Assembler(HasmParser parser, IEnumerable<string> listing)
		{
			_listing = listing.Select(l => Instruction.ParseFromLine(parser, l)).ToList();
		}

		public byte[] Process()
		{
			return null;
		}
	}
}