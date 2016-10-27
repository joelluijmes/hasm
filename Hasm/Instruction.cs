namespace hasm
{
	internal class Instruction
	{
		public Instruction(string label, string input, string comment)
		{
			Label = label;
			Input = input;
			Comment = comment;
		}

		public string Label { get; }
		public string Input { get; }
		public string Comment { get; }
	}

	internal class EncodedInstruction : Instruction
	{
		public EncodedInstruction(Instruction instruction, byte[] encoded) : this(instruction.Label, instruction.Input, instruction.Comment, encoded)
		{ }

		public EncodedInstruction(string label, string input, string comment, byte[] encoded) : base(label, input, comment)
		{
			Encoded = encoded;
		}

		internal byte[] Encoded { get;  }
	}
}