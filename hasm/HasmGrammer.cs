using System;
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

		private static readonly ValueRule<string> _opcodeMask;
		private static readonly ValueRule<string> _sourceRegisterMask;
		private static readonly ValueRule<string> _destinationRegisterMask;
		private static readonly ValueRule<string> _immmediateMask;

		private static readonly IDictionary<OperandTypes, Rule> _knownRules;
		private static readonly IDictionary<OperandTypes, EncodeValue> _knownEncodings;

		private readonly IDictionary<string, OperandTypes> _defines;

		static HasmGrammer()
		{
			_opcodeMask = MaskEncodingRule('1');
			_sourceRegisterMask = MaskEncodingRule('r');
			_destinationRegisterMask = MaskEncodingRule('d');
			_immmediateMask = MaskEncodingRule('k');
			_logger.Debug("Created the mask rules");

			_knownRules = new Dictionary<OperandTypes, Rule>
			{
				[OperandTypes.DestinationRegister] = GeneralRegisterRule("dst"),
				[OperandTypes.SourceRegister] = GeneralRegisterRule("src"),
				[OperandTypes.Immediate] = ImmediateRule(8)
			};

			_knownEncodings = new Dictionary<OperandTypes, EncodeValue>
			{
				[OperandTypes.DestinationRegister] = (encoding, value) => InsertEncodingValue(encoding, value, _destinationRegisterMask, 'd'),
				[OperandTypes.SourceRegister] = (encoding, value) => InsertEncodingValue(encoding, value, _sourceRegisterMask, 'r'),
				[OperandTypes.Immediate] = (encoding, value) => InsertEncodingValue(encoding, value, _immmediateMask, 'k')
			};
		}

		public HasmGrammer(IDictionary<string, OperandTypes> defines)
		{
			if (defines == null)
				throw new ArgumentNullException(nameof(defines));
			_defines = defines;
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
			return ConstantValue(encoding, MatchString(opcode, true)); // when it matches the opcode give its encoding 
		}

		private Rule ParseOperands(Instruction instruction)
		{
			var operands = GetOperands(instruction.Grammar);
			return operands.Select(o => ParseOperand(o, instruction)) // make operand rules from the strings
				.Aggregate((total, next) => total + MatchChar(',') + next); // merge the rules sepearted by a ,
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
			if ((type == OperandTypes.Unkown) || !_knownEncodings.TryGetValue(type, out encoder))
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
		
		private static int InsertEncodingValue(string encoding, string value, ValueRule<string> registerMask, char mask)
		{
			var opcodeBinary = registerMask.FirstValue(encoding); // gets the binary representation of the encoding
			var index = opcodeBinary.IndexOf(mask); // finds the first occurance of the mask
			var nextIndex = opcodeBinary.IndexOf('0', index); // and the last
			if (nextIndex == -1)
				nextIndex = opcodeBinary.Length; // could be that it ended with the mask so we set it to the length of total encoding

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

		private static Rule ImmediateRule(int count)
		{
			Func<string, string> converter = s =>
			{
				var number = int.Parse(s); // convert it to a number
				return Convert.ToString(number, 2).PadLeft(count, '0'); // then use Convert to make it binary
			};

			var rule = ConvertToValue(converter, Digits);
			return FirstValue<string>($"Imm{count}", rule);
		}

		private static Rule GeneralRegisterRule(string name)
		{
			Func<string, string> converter = s =>
			{
				var number = int.Parse(s); // convert it to a number
				return Convert.ToString(number, 2).PadLeft(3, '0'); // then use Convert to make it binary
			};

			var range = Enumerable.Range(0, 8) // generate 0 .. 7
				.Select(i => i.ToString()[0]) // convert them to strings
				.Select(i => ConvertToValue(converter, MatchChar(i))); // apply the converter

			return FirstValue<string>(name, MatchChar('R', true) + Or(range));
		}

		private static IEnumerable<string> GetOperands(string grammar)
			=> grammar.Replace(GetOpcode(grammar), "") // remove operand from grammar
				.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries) // operands are split with a ,
				.Select(s => s.Trim()); // remove any whitespace 

		private static string GetOpcode(string grammar)
			=> grammar.Substring(0, grammar.IndexOf(' ')).Trim();

		private delegate int EncodeValue(string encoding, string value);

		internal enum OperandTypes
		{
			Unkown,
			Immediate,
			SourceRegister,
			DestinationRegister
		}
	}
}