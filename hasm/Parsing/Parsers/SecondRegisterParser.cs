namespace hasm.Parsing.Parsers
{
	internal class SecondRegisterParser : BaseRegisterParser
	{
		private const string NAME = "REG2";
		private const char MASK = 'r';

		public SecondRegisterParser() : base(NAME, MASK)
		{
		}

		public override OperandType OperandType => OperandType.SecondRegister;
	}
}