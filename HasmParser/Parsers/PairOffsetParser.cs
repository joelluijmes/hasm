﻿using NLog;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Parsers
{
	/// <summary>
	/// Parses operand that take offset from special pair
	/// </summary>
	/// <seealso cref="hasm.Parsing.Parsers.IParser" />
	internal sealed class PairOffsetParser : IParser
	{
		private const string NAME = "PAIR+k";
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private static readonly IParser _pairParser = new PairParser();
		private static readonly IParser _immediateParser = new Immediate6Parser();
		private Rule _rule;

		/// <summary>
		/// Gets the type of the operand.
		/// </summary>
		/// <value>
		/// OperandType.PairOffset
		/// </value>
		public OperandType OperandType => OperandType.PairOffset;

		/// <summary>
		/// Creates the rule for this parser.
		/// </summary>
		/// <param name="encoding">The encoding.</param>
		/// <returns>
		/// Rule which fills in the encoding
		/// </returns>
		public Rule CreateRule(string encoding)
		{
			if (_rule != null)
				return _rule;

			_rule = _pairParser.CreateRule(encoding) + _immediateParser.CreateRule(encoding);
			return _rule;
		}
	}
}