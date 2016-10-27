namespace hasm.Parsers
{
	internal class FirstGeneralRegisterParser : BaseGeneralRegisterParser
	{
		private const string NAME = "REG1";
		private const char MASK = 'd';

		public FirstGeneralRegisterParser() : base(NAME, MASK)
		{
		}

		public override OperandType OperandType => OperandType.FirstGeneralRegister;
	}
}