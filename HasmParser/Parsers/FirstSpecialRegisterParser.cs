namespace hasm.Parsers
{
	internal class FirstSpecialRegisterParser : BaseSpecialRegisterParser
	{
		private const string NAME = "SPC1";
		private const char MASK = 'D';

		public FirstSpecialRegisterParser() : base(NAME, MASK)
		{
		}

		public override OperandType OperandType => OperandType.FirstSpecialRegister;
	}
}