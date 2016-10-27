namespace hasm.Parsers
{
	internal sealed class Immediate6Parser : BaseImmediateParser
	{
		private const int SIZE = 6;

		public Immediate6Parser() : base($"IMM{SIZE}", SIZE)
		{
		}

		public override OperandType OperandType => OperandType.Immediate6;
	}
}