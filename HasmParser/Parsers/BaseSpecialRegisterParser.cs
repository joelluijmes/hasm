using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsers
{
	internal abstract class BaseSpecialRegisterParser : BaseRegisterParser
	{
		protected BaseSpecialRegisterParser(string name, char mask) : base(name, mask)
		{
		}

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