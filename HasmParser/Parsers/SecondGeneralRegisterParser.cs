namespace hasm.Parsing.Parsers
{
	internal class SecondGeneralRegisterParser : BaseGeneralRegisterParser
	{
		private const string NAME = "REG2";
		private const char MASK = 'r';

		public SecondGeneralRegisterParser() : base(NAME, MASK)
		{
		}

		public override OperandType OperandType => OperandType.SecondGeneralRegister;
	}
}