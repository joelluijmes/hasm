namespace hasm.Parsing.Parsers
{
	/// <summary>
	/// 12 bit immediate parser
	/// </summary>
	/// <seealso cref="hasm.Parsing.Parsers.BaseImmediateParser" />
	internal sealed class Immediate12Parser : BaseImmediateParser
	{
		private const int SIZE = 12;

		/// <summary>
		/// Initializes a new instance of the <see cref="Immediate12Parser"/> class.
		/// </summary>
		public Immediate12Parser() : base($"IMM{SIZE}", SIZE)
		{
		}

		/// <summary>
		/// Gets the type of the operand.
		/// </summary>
		/// <value>
		/// OperandType.Immediate12
		/// </value>
		public override OperandType OperandType => OperandType.Immediate12;
	}
}