using hasm.Parsing.Models;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.OperandParsers
{
	/// <summary>
	/// Interface for predefined parsers
	/// </summary>
	internal interface IOperandParser
	{
		/// <summary>
		/// Gets the type of the operand.
		/// </summary>
		/// <value>
		/// The type of the operand.
		/// </value>
		OperandType OperandType { get; }

		/// <summary>
		/// Creates the rule for this parser.
		/// </summary>
		/// <param name="encoding">The encoding.</param>
		/// <returns>Rule which fills in the encoding</returns>
		Rule CreateRule(string encoding);
	}
}