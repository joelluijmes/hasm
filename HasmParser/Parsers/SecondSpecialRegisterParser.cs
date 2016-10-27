namespace hasm.Parsers
{
	internal class SecondSpecialRegisterParser : BaseSpecialRegisterParser
	{
		private const string NAME = "SPC2";
		private const char MASK = 'R';

		public SecondSpecialRegisterParser() : base(NAME, MASK)
		{
		}

		public override OperandType OperandType => OperandType.SecondSpecialRegister;
	}
}