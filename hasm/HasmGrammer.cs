using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm
{
	internal sealed class HasmGrammer : Grammar
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IDictionary<string, Rule> _defines;

		public HasmGrammer(IDictionary<string, Rule> defines)
		{
			if (defines == null)
				throw new ArgumentNullException(nameof(defines));
			_defines = defines;
		}

		public static Rule GeneralRegister()
		{
			var range = Enumerable.Range(0, 8)
				.Select(i => i.ToString()[0])
				.Select(i => ConvertToValue("r" + i, int.Parse, MatchChar(i)));

			return FirstValue<int>("GeneralRegister", MatchChar('R', true) + Or(range));
		}
		
		public Rule ParseInstruction(Instruction instruction)
		{
			_logger.Info($"Parsing {instruction}..");
			var rule = ParseOpcode(instruction) + Whitespace + ParseOperands(instruction);
			_logger.Info($"Parsed {instruction}: {rule}");

			return rule;
		}

		private Rule ParseOpcode(Instruction instruction)
		{
			var opcode = GetOpcode(instruction.Grammar);
			return MatchString(opcode, true);
		}

		private Rule ParseOperands(Instruction instruction)
		{
			Rule rule = null;
			var operands = GetOperands(instruction.Grammar);
			foreach (var operand in operands)
			{
				Rule tmp;
				// try to get existing rule for this operand
				if (!_defines.TryGetValue(operand, out tmp))
				{
					_logger.Debug($"No definition found for {operand}");
					_logger.Warn($"Assuming that operand '{operand}' is MatchString");

					// rule was not found
					tmp = MatchString(operand, true);
				}
				else _logger.Debug($"Found definition for {operand}: {tmp}");

				rule = rule == null
					? tmp
					: rule + MatchChar(',') + tmp;
			}

			return rule;
		}

		private static IEnumerable<string> GetOperands(string grammar)
			=> grammar.Replace(GetOpcode(grammar), "") // remove operand from grammar
				.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries) // operands are split with a ,
				.Select(s => s.Trim()); // remove any whitespace 

		private static string GetOpcode(string grammar)
			=> grammar.Substring(0, grammar.IndexOf(' ')).Trim();
	}
}