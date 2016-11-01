using hasm.Parsing.Models;

namespace hasm.Parsing.OperandParsers
{
	/// <summary>
	/// Second general purpose register (REG2)
	/// </summary>
	/// <seealso cref="BaseGeneralRegisterParser" />
	internal class SecondGeneralRegisterParser : BaseGeneralRegisterParser
	{
		private const string NAME = "REG2";
		private const char MASK = 'r';

		/// <summary>
		/// Initializes a new instance of the <see cref="SecondGeneralRegisterParser"/> class.
		/// </summary>
		public SecondGeneralRegisterParser() : base(NAME, MASK)
		{
		}

		/// <summary>
		/// Gets the type of the operand.
		/// </summary>
		/// <value>
		///  OperandType.SecondGeneralRegister
		/// </value>
		public override OperandType OperandType => OperandType.SecondGeneralRegister;
	}
}