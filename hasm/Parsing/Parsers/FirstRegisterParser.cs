namespace hasm.Parsing.Parsers
{
	internal class FirstRegisterParser : BaseRegisterParser
	{
		private const string NAME = "REG1";
		private const char MASK = 'd';

		public FirstRegisterParser() : base(NAME, MASK)
		{
		}

		public override OperandType OperandType => OperandType.FirstRegister;
	}
}