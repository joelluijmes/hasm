﻿using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using ParserLib;
using ParserLib.Evaluation;
using ParserLib.Evaluation.Rules;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm
{
	internal sealed class HasmGrammer : Grammar
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private static ValueRule<string> _opcodeEncodingRule;
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

		private ValueRule<int> ParseOpcode(Instruction instruction)
		{
			var opcode = GetOpcode(instruction.Grammar);
			var encoding = OpcodeEncoding(instruction.Encoding);
			return ConstantValue(encoding, MatchString(opcode, true));	// when it matches the opcode give its encoding 
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

		private static int OpcodeEncoding(string encoding)
		{
			var rule = OpcodeEncodingRule();
			var opcodeBinary = rule.FirstValue(encoding); // gets the binary representation of the encoding
			var result = Convert.ToInt32(opcodeBinary, 2);
			
			_logger.Info($"Opcode for {encoding} is {result}");
			return result;
		}

		private static ValueRule<string> OpcodeEncodingRule()
		{
			if (_opcodeEncodingRule != null)
				return _opcodeEncodingRule;

			var one = ConstantValue("1", MatchChar('1')); // matches the one's as 1
			var rest = ConstantValue("0", MatchAnyChar()); // treat the rest as an zero

			_opcodeEncodingRule = Accumulate<string>((cur, next) => cur + next, MatchWhile(one | rest)); // merge the encoding
			_logger.Debug(() => $"Create opcode encoding rule{Environment.NewLine}{_opcodeEncodingRule.PrettyFormat()}");

			return _opcodeEncodingRule;
		}

		private static IEnumerable<string> GetOperands(string grammar)
			=> grammar.Replace(GetOpcode(grammar), "") // remove operand from grammar
				.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries) // operands are split with a ,
				.Select(s => s.Trim()); // remove any whitespace 

		private static string GetOpcode(string grammar)
			=> grammar.Substring(0, grammar.IndexOf(' ')).Trim();
	}
}