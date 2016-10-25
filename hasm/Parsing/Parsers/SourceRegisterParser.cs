namespace hasm.Parsing.Parsers
{
	internal class SourceRegisterParser : BaseRegisterParser
	{
		private const string NAME = "SRC";
		private const char MASK = 'r';

		public SourceRegisterParser() : base(NAME, MASK)
		{
		}

		public override OperandType OperandType => OperandType.SourceRegister;
	}
}