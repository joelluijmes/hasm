using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using hasm.Parsing.Properties;
using NLog;
using OfficeOpenXml;
using ParserLib;
using ParserLib.Evaluation;
using ParserLib.Evaluation.Rules;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing
{
	public class HasmParser
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly HasmGrammar _grammar;
		private readonly IList<InstructionEncoding> _instructions;

		public HasmParser(HasmGrammar grammar)
		{
			_logger.Info($"{grammar.Definitions.Count} definitions");
			foreach (var pair in grammar.Definitions)
				_logger.Debug($"{pair.Key}: {pair.Value}");

			_grammar = grammar;
			_instructions = ParseInstructions().ToList(); // parseInstructions is deferred
			_logger.Info($"hasm knows {_instructions.Count} instructions");
		}

		public ValueRule<byte[]> FindRule(string input)
		{
			if (string.IsNullOrEmpty(input))
				throw new ArgumentNullException(nameof(input));

			var instruction = FindInstructionEncoding(input);
			var rule = _grammar.ParseInstruction(instruction);
			_logger.Debug(() => $"{input} - Found rule:{Environment.NewLine}{rule.PrettyFormat()}");

			return rule;
		}

		private InstructionEncoding FindInstructionEncoding(string input)
		{
			var opcode = HasmGrammar.Opcode.FirstValue(FormatInput(input));
			var instruction = _instructions.FirstOrDefault(i => i.Grammar.StartsWith(opcode.ToUpper()));
			return instruction;
		}

		public byte[] Encode(string input)
		{
			if (string.IsNullOrEmpty(input))
				throw new ArgumentNullException(nameof(input));

			input = FormatInput(input);
			var rule = FindRule(input);
			return rule.FirstValue(input);
		}

		public bool TryEncode(string input, out byte[] encoded)
		{
			if (string.IsNullOrEmpty(input))
				throw new ArgumentNullException(nameof(input));

			input = FormatInput(input);
			var instruction = FindInstructionEncoding(input);
			if (instruction == null)
			{
				encoded = null;
				return false;
			}

			var rule = _grammar.ParseInstruction(instruction);
			if (rule.Match(input))
			{
				encoded = rule.FirstValue(input);
				return true;
			}

			encoded = new byte[instruction.Count];
			return false;
		}
		
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