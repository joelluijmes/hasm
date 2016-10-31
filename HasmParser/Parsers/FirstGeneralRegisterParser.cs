namespace hasm.Parsing.Parsers
{
	/// <summary>
	/// First general purpose register (REG1)
	/// </summary>
	/// <seealso cref="hasm.Parsing.Parsers.BaseGeneralRegisterParser" />
	internal class FirstGeneralRegisterParser : BaseGeneralRegisterParser
	{
		private const string NAME = "REG1";
		private const char MASK = 'd';

		/// <summary>
		/// Initializes a new instance of the <see cref="FirstGeneralRegisterParser"/> class.
		/// </summary>
		public FirstGeneralRegisterParser() : base(NAME, MASK)
		{
		}

		/// <summary>
		/// Gets the type of the operand.
		/// </summary>
		/// <value>
		/// OperandType.FirstGeneralRegister
		/// </value>
		public override OperandType OperandType => OperandType.FirstGeneralRegister;
	}
}