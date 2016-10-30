namespace hasm
{
	internal class Instruction
	{
		public Instruction(string input)
		{
			Input = input;
		}

		public Instruction(string input, byte[] encoding)
		{
			Input = input;
			Encoding = encoding;
			Completed = true;
		}

		public Instruction(string label, string input, byte[] encoding = null, bool completed = false)
		{
			Label = label;
			Input = input;
			Encoding = encoding;
			Completed = completed;
		}

		public string Label { get; }
		public string Input { get; set; }
		public byte[] Encoding { get; set; }
		public bool Completed { get; set; }
		public int Address { get; set; }
	}
}