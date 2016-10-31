namespace hasm
{
	/// <summary>
	/// Model used to assemble a line into the correct encoding.
	/// </summary>
	internal class Instruction
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="Instruction"/> class.
		/// </summary>
		/// <param name="input">The instruction.</param>
		public Instruction(string input)
		{
			Input = input;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Instruction"/> class.
		/// </summary>
		/// <param name="input">The instruction.</param>
		/// <param name="encoding">The encoded instruction.</param>
		public Instruction(string input, byte[] encoding)
		{
			Input = input;
			Encoding = encoding;
			Completed = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Instruction"/> class.
		/// </summary>
		/// <param name="label">The label.</param>
		/// <param name="input">The instruction.</param>
		/// <param name="encoding">The encoded instruction.</param>
		/// <param name="completed">if set to <c>true</c> [completed] the assembler is done encoding it.</param>
		public Instruction(string label, string input, byte[] encoding = null, bool completed = false)
		{
			Label = label;
			Input = input;
			Encoding = encoding;
			Completed = completed;
		}

		/// <summary>
		/// Gets the label in front of instruction.
		/// </summary>
		/// <value>
		/// The label.
		/// </value>
		public string Label { get; }

		/// <summary>
		/// Gets or sets the instruction to be parsed.
		/// </summary>
		/// <value>
		/// The input.
		/// </value>
		public string Input { get; set; }

		/// <summary>
		/// Gets or sets the encoded form of instruction.
		/// </summary>
		/// <value>
		/// The encoding.
		/// </value>
		public byte[] Encoding { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Instruction"/> is completed parsing.
		/// </summary>
		/// <value>
		///   <c>true</c> if completed; otherwise, <c>false</c>.
		/// </value>
		public bool Completed { get; set; }

		/// <summary>
		/// Gets or sets the address when the listing is encoded.
		/// </summary>
		/// <value>
		/// The address.
		/// </value>
		public int Address { get; set; }
	}
}