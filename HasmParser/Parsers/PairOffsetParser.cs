using NLog;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Parsers
{
	internal sealed class PairOffsetParser : IParser
	{
		private const string NAME = "PAIR+k";
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private static readonly IParser _pairParser = new PairParser();
		private static readonly IParser _immediateParser = new Immediate6Parser();
		private Rule _rule;

		public OperandType OperandType => OperandType.PairOffset;

		public Rule CreateRule(string encoding)
		{
			if (_rule != null)
				return _rule;

			_rule = _pairParser.CreateRule(encoding) + _immediateParser.CreateRule(encoding);
			return _rule;
		}
	}
}