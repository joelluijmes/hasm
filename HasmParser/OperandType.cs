namespace hasm.Parsing
{
	/// <summary>
	/// Possible operand types to link known definitions.
	/// </summary>
	public enum OperandType
	{
		Unkown,
		Immediate6,
		Immediate8,
		Immediate12,
		SourceRegister,
		DestinationRegister,
		FirstGeneralRegister,
		FirstSpecialRegister,
		SecondGeneralRegister,
		SecondSpecialRegister,
		BranchIf,
		PairOffset,
		Pair
	}
}