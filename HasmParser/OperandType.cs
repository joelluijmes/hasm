namespace hasm.Parsing
{
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