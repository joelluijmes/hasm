namespace hasm
{
	internal sealed class Instruction
	{
		public string Label { get; set; }
		public int Address { get; set; }
		public string Input { get; set; }
		public string Comment { get; set; }
	}
}