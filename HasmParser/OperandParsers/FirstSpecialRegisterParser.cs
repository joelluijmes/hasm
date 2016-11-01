using hasm.Parsing.Models;

namespace hasm.Parsing.OperandParsers
{
	/// <summary>
	/// First special purpose register (SPC1)
	/// </summary>
	/// <seealso cref="BaseSpecialRegisterParser" />
	internal class FirstSpecialRegisterParser : BaseSpecialRegisterParser
	{
		private const string NAME = "SPC1";
		private const char MASK = 'D';

		/// <summary>
		/// Initializes a new instance of the <see cref="FirstSpecialRegisterParser"/> class.
		/// </summary>
		public FirstSpecialRegisterParser() : base(NAME, MASK)
		{
		}

		/// <summary>
		/// Gets the type of the operand.
		/// </summary>
		/// <value>
		/// OperandType.FirstSpecialRegister
		/// </value>
		public override OperandType OperandType => OperandType.FirstSpecialRegister;
	}
}