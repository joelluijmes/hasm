namespace hasm.Parsing.Parsers
{
	internal abstract class BaseRegisterParser : BaseParser
	{
		private const int SIZE_REGISTER_ENCODING = 3;

		protected BaseRegisterParser(string name, char mask) : base(name, mask, SIZE_REGISTER_ENCODING)
		{
		}
	}
}