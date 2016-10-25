namespace hasm.Parsing.Parsers
{
	internal sealed class Immediate8Parser : BaseImmediateParser
	{
		private const int SIZE = 8;

		public Immediate8Parser() : base($"IMM{SIZE}", SIZE)
		{
		}

		public override OperandType OperandType => OperandType.Immediate8;
	}
}