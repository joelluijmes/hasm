namespace hasm.Parsing.Parsers
{
	internal class DestinationRegisterParser : BaseRegisterParser
	{
		private const string NAME = "DST";
		private const char MASK = 'd';

		public DestinationRegisterParser() : base(NAME, MASK)
		{
		}

		public override OperandType OperandType => OperandType.DestinationRegister;
	}
}