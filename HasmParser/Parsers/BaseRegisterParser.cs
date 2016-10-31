namespace hasm.Parsing.Parsers
{
	/// <summary>
	/// Provides base for Register parsers.
	/// </summary>
	/// <seealso cref="hasm.Parsing.Parsers.BaseParser" />
	internal abstract class BaseRegisterParser : BaseParser
	{
		private const int SIZE_REGISTER_ENCODING = 3;

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseRegisterParser"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="mask">The mask in the encoding.</param>
		protected BaseRegisterParser(string name, char mask) : base(name, mask, SIZE_REGISTER_ENCODING)
		{
		}
	}
}