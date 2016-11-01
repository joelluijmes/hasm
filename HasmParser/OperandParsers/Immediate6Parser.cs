using hasm.Parsing.Models;

namespace hasm.Parsing.OperandParsers
{
	/// <summary>
	/// 6 bit immediate parser
	/// </summary>
	/// <seealso cref="BaseImmediateParser" />
	internal sealed class Immediate6Parser : BaseImmediateParser
	{
		private const int SIZE = 6;

		/// <summary>
		/// Initializes a new instance of the <see cref="Immediate6Parser"/> class.
		/// </summary>
		public Immediate6Parser() : base($"IMM{SIZE}", SIZE)
		{
		}

		/// <summary>
		/// Gets the type of the operand.
		/// </summary>
		/// <value>
		/// OperandType.Immediate6
		/// </value>
		public override OperandType OperandType => OperandType.Immediate6;
	}
}