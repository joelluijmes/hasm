using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.OperandParsers
{
	/// <summary>
	/// Provices base class for speciael purpose register parsers
	/// </summary>
	/// <seealso cref="BaseRegisterParser" />
	internal abstract class BaseSpecialRegisterParser : BaseRegisterParser
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseSpecialRegisterParser"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="mask">The mask in the encoding.</param>
		protected BaseSpecialRegisterParser(string name, char mask) : base(name, mask)
		{
		}

		/// <summary>
		/// Creates the match rule for the grammar.
		/// </summary>
		/// <returns>
		/// The rule.
		/// </returns>
		protected override Rule CreateMatchRule()
		{
			// TODO: from encoding sheet
			var sp = Grammar.ConstantValue("000", Grammar.MatchString("SP", true));
			var pc = Grammar.ConstantValue("001", Grammar.MatchString("PC", true));
			var mdr = Grammar.ConstantValue("010", Grammar.MatchString("MDR", true));
			var y = Grammar.ConstantValue("110", Grammar.MatchString("Y", true));
			var z = Grammar.ConstantValue("111", Grammar.MatchString("Z", true));

			return Grammar.FirstValue<string>(sp | pc | mdr | y | z);
		}
	}
}