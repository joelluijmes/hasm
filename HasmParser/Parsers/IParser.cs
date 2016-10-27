using ParserLib.Parsing.Rules;

namespace hasm.Parsers
{
	internal interface IParser
	{
		OperandType OperandType { get; }

		Rule CreateRule(string encoding);
	}
}