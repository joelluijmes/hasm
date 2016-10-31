namespace hasm.Parsing.Parsers
{
	/// <summary>
	/// 8 bit immediate parser
	/// </summary>
	/// <seealso cref="hasm.Parsing.Parsers.BaseImmediateParser" />
	internal sealed class Immediate8Parser : BaseImmediateParser
	{
		private const int SIZE = 8;

		/// <summary>
		/// Initializes a new instance of the <see cref="Immediate8Parser"/> class.
		/// </summary>
		public Immediate8Parser() : base($"IMM{SIZE}", SIZE)
		{
		}

		/// <summary>
		/// Gets the type of the operand.
		/// </summary>
		/// <value>
		/// OperandType.Immediate8
		/// </value>
		public override OperandType OperandType => OperandType.Immediate8;
	}
}