using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Parsers
{
	internal interface IParser
	{
		OperandType OperandType { get; }

		Rule CreateRule(string encoding);
	}
}