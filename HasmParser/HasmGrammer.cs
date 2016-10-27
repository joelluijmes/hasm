using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hasm.Parsing.Parsers;
using NLog;
using ParserLib;
using ParserLib.Evaluation;
using ParserLib.Evaluation.Rules;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing
{
	internal sealed class HasmGrammer : Grammar
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private static readonly IDictionary<OperandType, IParser> _knownParsers;
		private static readonly ValueRule<string> _opcodemaskRule;

		public static readonly ValueRule<string> Operand = Text("Operand", MatchWhile(Label));

		private readonly IDictionary<string, OperandType> _defines;

		static HasmGrammer()
		{
			_knownParsers = Assembly.GetExecutingAssembly()
				.GetTypes() // get all types
				.Where(t => t.IsClass && !t.IsAbstract && typeof(IParser).IsAssignableFrom(t)) // which are parsers
				.Select(t => (IParser) Activator.CreateInstance(t)) // create an instance of them
				.ToDictionary(p => p.OperandType); // and make it a dictionary :)

			_opcodemaskRule = CreateMaskRule('1');
			_logger.Info($"Found {_knownParsers.Count} parsers");
		}

		public HasmGrammer(IDictionary<string, OperandType> defines)
		{
			if (defines == null)
				throw new ArgumentNullException(nameof(defines));
			_defines = defines;
		}

		public Rule ParseInstruction(Instruction instruction)
		{
			_logger.Info($"Parsing {instruction}..");
			var rule = ParseOpcode(instruction);
			var operands = ParseOperands(instruction);

			if (operands != null)
				rule += Whitespace + operands;

			_logger.Info($"Parsed {instruction}: {rule}");
			return Accumulate<int>((current, next) => current | next, rule);
		}

		private static Rule ParseOpcode(Instruction instruction)
		{
			var opcode = Operand.FirstValue(instruction.Grammar);
			var encoding = OpcodeEncoding(instruction.Encoding);
			return ConstantValue(encoding, MatchString(opcode, true)); // when it matches the opcode give its encoding 
		}

		private Rule ParseOperands(Instruction instruction)
		{
			var operands = GetOperands(instruction.Grammar);
			if (!operands.Any()) // instruction without oprands
				return null;

			return operands.Select(o => ParseOperand(o, instruction.Encoding)) // make operand rules from the strings
				.Aggregate((total, next) => total + MatchChar(',') + next); // merge the rules sepearted by a ,
		}

		private Rule ParseOperand(string operand, string encoding)
		{
			IParser parser;
			OperandType type;

			// get the parser for this opernad type
			if (!_defines.TryGetValue(operand, out type) || !_knownParsers.TryGetValue(type, out parser) || (type == OperandType.Unkown))
				throw new InvalidOperationException($"Impossible to encode for operand {operand}");

			_logger.Debug($"Found parser for {operand}: {parser}");
			return parser.CreateRule(encoding);
		}

		private static int OpcodeEncoding(string encoding)
		{
			var opcodeBinary = _opcodemaskRule.FirstValue(encoding); // gets the binary representation of the encoding
			var result = Convert.ToInt32(opcodeBinary, 2);

			_logger.Info($"Opcode for {encoding} is {result}");
			return result;
		}

		private static string[] GetOperands(string grammar)
		{
			var operand = Operand.FirstValue(grammar);
			return grammar.Replace(operand, "") // remove operand from grammar
				.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries) // operands are split with a ,
				.Select(s => s.Trim())
				.ToArray();
		}

		// remove any whitespace 

		internal static ValueRule<string> CreateMaskRule(char mask)
		{
			var matched = ConstantValue(mask.ToString(), MatchChar(mask)); // matches only the mask
			var rest = ConstantValue("0", MatchAnyChar()); // treat the rest as an zero

			var rule = Accumulate<string>((cur, next) => cur + next, MatchWhile(matched | rest)); // merge the encoding
			_logger.Debug(() => $"Created encoding-mask ('{mask}') rule{Environment.NewLine}{rule.PrettyFormat()}");

			return rule;
		}
	}
}