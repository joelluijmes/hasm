﻿namespace hasm.Parsing.Parsers
{
	/// <summary>
	/// Second special purpose register.
	/// </summary>
	/// <seealso cref="hasm.Parsing.Parsers.BaseSpecialRegisterParser" />
	internal class SecondSpecialRegisterParser : BaseSpecialRegisterParser
	{
		private const string NAME = "SPC2";
		private const char MASK = 'R';

		/// <summary>
		/// Initializes a new instance of the <see cref="SecondSpecialRegisterParser"/> class.
		/// </summary>
		public SecondSpecialRegisterParser() : base(NAME, MASK)
		{
		}

		/// <summary>
		/// Gets the type of the operand.
		/// </summary>
		/// <value>
		/// OperandType.SecondSpecialRegister
		/// </value>
		public override OperandType OperandType => OperandType.SecondSpecialRegister;
	}
}