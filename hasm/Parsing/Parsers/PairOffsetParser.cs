using System;
using NLog;
using ParserLib;
using ParserLib.Evaluation;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Parsers
{
	internal sealed class PairOffsetParser : IParser
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private const string NAME = "PAIR+k";
		private static readonly IParser _pairParser = new PairParser();
		private static readonly IParser _immediateParser = new Immediate6Parser();
		private Rule _rule;

		public PairOffsetParser()
		{
		}

		public OperandType OperandType => OperandType.PairOffset;
		public Rule CreateRule(string encoding)
		{
			if (_rule != null)
				return _rule;

			_rule = _pairParser.CreateRule(encoding) +  _immediateParser.CreateRule(encoding);
			return _rule;
		}
	}
}