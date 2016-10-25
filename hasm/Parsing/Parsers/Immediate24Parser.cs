namespace hasm.Parsing.Parsers
{
	internal sealed class Immediate12Parser : BaseImmediateParser
	{
		private const int SIZE = 12;

		public Immediate12Parser() : base($"IMM{SIZE}", SIZE)
		{
		}

		public override OperandType OperandType => OperandType.Immediate12;
	}
}