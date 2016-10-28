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
		public EncodedInstruction(Instruction instruction, byte[] encoded, bool completed = false) : base(instruction.Label, instruction.Input, instruction.Comment)
		{
			Encoded = encoded;
			Completed = completed;
		}

		public EncodedInstruction(string input, byte[] encoded) : base(null, input, null)
		{
			Encoded = encoded;
			Completed = true;
		}

		public byte[] Encoded { get; }
		public bool Completed { get; }
	}
}