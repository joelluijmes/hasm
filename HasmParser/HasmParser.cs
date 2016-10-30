using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using hasm.Parsing.Properties;
using NLog;
using OfficeOpenXml;
using ParserLib.Evaluation;
using ParserLib.Evaluation.Rules;

namespace hasm.Parsing
{
	public class HasmParser
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly HasmGrammar _grammar;
		private readonly IList<InstructionEncoding> _instructions;
		private readonly Dictionary<string, ValueRule<byte[]>> _rules;

		public HasmParser(HasmGrammar grammar)
		{
			foreach (var pair in grammar.Definitions)
				_logger.Debug($"{pair.Key}: {pair.Value}");

			_grammar = grammar;
			_instructions = ParseInstructions().ToList(); // parseInstructions is deferred
			_rules = new Dictionary<string, ValueRule<byte[]>>();
			_logger.Info($"Learned {_instructions.Count} instructions");
		}

		public byte[] Encode(string input)
		{
			byte[] encoded;
			if (!TryEncode(input, out encoded))
				throw new NotImplementedException();

			return encoded;
		}

		public bool TryEncode(string input, out byte[] encoded)
		{
			if (string.IsNullOrEmpty(input))
				throw new ArgumentNullException(nameof(input));

			input = FormatInput(input);
			var opcode = HasmGrammar.Opcode.FirstValue(input);

			var rule = FindRule(opcode);
			if (rule == null)
			{
				encoded = null;
				return false;
			}

			if (rule.Match(input))
			{
				encoded = rule.FirstValue(input);
				return true;
			}

			var instruction = FindInstructionEncoding(opcode);
			encoded = new byte[instruction.Count];
			return false;
		}

		private ValueRule<byte[]> FindRule(string opcode)
		{
			ValueRule<byte[]> rule;
			if (_rules.TryGetValue(opcode, out rule))
			{
				_logger.Debug(() => $"Found rule: {rule}");
				return rule;
			}

			var instruction = FindInstructionEncoding(opcode);
			if (instruction == null)
				return null;

			rule = _grammar.ParseInstruction(instruction);
			_logger.Debug(() => $"Created rule: {rule}");

			_rules[opcode] = rule;
			return rule;
		}

		private InstructionEncoding FindInstructionEncoding(string opcode)
			=> _instructions.FirstOrDefault(i => i.Grammar.StartsWith(opcode.ToUpper()));

		private static string FormatInput(string input)
		{
			var opcode = HasmGrammar.Opcode.FirstValue(input);
			var operands = input.Substring(opcode.Length);
			operands = new Regex("\\s+").Replace(operands, "");

			return opcode + " " + operands;
		}

		private static IEnumerable<InstructionEncoding> ParseInstructions()
		{
			using (var stream = new MemoryStream(Resources.Instructionset))
			{
				using (var package = new ExcelPackage(stream))
				{
					var sheet = package.Workbook.Worksheets.First();

					var start = sheet.Dimension.Start;
					var end = sheet.Dimension.End;

					for (var row = start.Row + 1; row <= end.Row; ++row)
					{
						var range = sheet.Cells[row, 1, row, end.Column];
						var instruction = InstructionEncoding.Parse(range);

						_logger.Debug($"Added: {instruction}");
						yield return instruction;
					}
				}
			}
		}
	}
}