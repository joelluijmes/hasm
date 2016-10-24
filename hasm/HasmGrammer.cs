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

		private delegate int EncodeValue(string encoding, string value);

		private static readonly ValueRule<string> _opcodeMask;
		private static readonly ValueRule<string> _sourceRegisterMask;
		private static readonly ValueRule<string> _destinationRegisterMask;

		private static readonly IDictionary<OperandTypes, Rule> _knownRules;
		private static readonly IDictionary<OperandTypes, EncodeValue> _knownEncodings;

		private readonly IDictionary<string, OperandTypes> _defines;
		
		public HasmGrammer(IDictionary<string, OperandTypes> defines)
		{
			if (defines == null)
				throw new ArgumentNullException(nameof(defines));
			_defines = defines;
		}

		static HasmGrammer()
		{
			_opcodeMask = MaskEncodingRule('1');
			_sourceRegisterMask = MaskEncodingRule('r');
			_destinationRegisterMask = MaskEncodingRule('d');
			_logger.Debug("Created the mask rules");

			_knownRules = new Dictionary<OperandTypes, Rule>
			{
				[OperandTypes.DestinationRegister] = GeneralRegisterRule("dst"),
				[OperandTypes.SourceRegister] = GeneralRegisterRule("src")
			};

			_knownEncodings = new Dictionary<OperandTypes, EncodeValue>
			{
				[OperandTypes.DestinationRegister] = DestinationRegisterEncoding,
				[OperandTypes.SourceRegister] = SourceRegisterEncoding
			};
		}

		public Rule ParseInstruction(Instruction instruction)
		{
			_logger.Info($"Parsing {instruction}..");
			var rule = ParseOpcode(instruction) + Whitespace + ParseOperands(instruction);
			_logger.Info($"Parsed {instruction}: {rule}");
			
			return Accumulate<int>((current, next) => current | next, rule);
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
				var tmp = ParseOperand(operand, instruction);

				rule = rule == null
					? tmp
					: rule + MatchChar(',') + tmp;
			}

			return rule;
		}

		private Rule ParseOperand(string operand, Instruction instruction)
		{
			Rule tmp;
			OperandTypes type;

			// try to get existing rule for this operand
			if (!_defines.TryGetValue(operand, out type) || !_knownRules.TryGetValue(type, out tmp))
			{
				_logger.Debug($"No definition found for {operand}");
				_logger.Warn($"Assuming that operand '{operand}' is MatchString");

				// rule was not found
				tmp = MatchString(operand, true);
			}

			EncodeValue encoder;
			if (type == OperandTypes.Unkown || !_knownEncodings.TryGetValue(type, out encoder))
				throw new InvalidOperationException($"Impossible to encode this {instruction}");

			_logger.Debug($"Found definition for {operand}: {tmp} with encoder {encoder.Method.Name}");
			return ConvertToValue(s => encoder(instruction.Encoding, tmp.FirstValue<string>(s)), tmp);
		}

		private static int OpcodeEncoding(string encoding)
		{
			var opcodeBinary = _opcodeMask.FirstValue(encoding); // gets the binary representation of the encoding
			var result = Convert.ToInt32(opcodeBinary, 2);

			_logger.Info($"Opcode for {encoding} is {result}");
			return result;
		}

		private static int SourceRegisterEncoding(string encoding, string value)
			=> RegisterEncoding(encoding, value, _sourceRegisterMask, 'r');

		private static int DestinationRegisterEncoding(string encoding, string value)
			=> RegisterEncoding(encoding, value, _destinationRegisterMask, 'd');

		private static int RegisterEncoding(string encoding, string value, ValueRule<string> registerMask, char mask)
		{
			var opcodeBinary = registerMask.FirstValue(encoding); // gets the binary representation of the encoding
			var index = opcodeBinary.IndexOf(mask);
			var nextIndex = opcodeBinary.IndexOf('0', index);
			if (nextIndex == -1)
				nextIndex = opcodeBinary.Length;
			var length = nextIndex - index;

			opcodeBinary = opcodeBinary.Remove(index, length).Insert(index, value);
			var result = Convert.ToInt32(opcodeBinary, 2);

			_logger.Info($"Register encoding ({mask}) for {encoding} is {result}");
			return result;
		}
		
		private static ValueRule<string> MaskEncodingRule(char mask)
		{
			var matched = ConstantValue(mask.ToString(), MatchChar(mask)); // matches only the mask
			var rest = ConstantValue("0", MatchAnyChar()); // treat the rest as an zero

			var rule = Accumulate<string>((cur, next) => cur + next, MatchWhile(matched | rest)); // merge the encoding
			_logger.Debug(() => $"Created encoding-mask ('{mask}') rule{Environment.NewLine}{rule.PrettyFormat()}");

			return rule;
		}

		private static Rule GeneralRegisterRule(string name)
		{
			Func<string, string> converter = s =>
			{
				var number = int.Parse(s);	// convert it to a number
				return Convert.ToString(number, 2).PadLeft(3, '0'); // then use Convert to make it binary
			};

			var range = Enumerable.Range(0, 8)
				.Select(i => i.ToString()[0])
				.Select(i => ConvertToValue(converter, MatchChar(i)));

			return FirstValue<string>(name, MatchChar('R', true) + Or(range));
		}

		private static IEnumerable<string> GetOperands(string grammar)
			=> grammar.Replace(GetOpcode(grammar), "") // remove operand from grammar
				.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries) // operands are split with a ,
				.Select(s => s.Trim()); // remove any whitespace 

		private static string GetOpcode(string grammar)
			=> grammar.Substring(0, grammar.IndexOf(' ')).Trim();

		internal enum OperandTypes
		{
			Unkown,
			Immediate,
			SourceRegister,
			DestinationRegister
		}
	}
}