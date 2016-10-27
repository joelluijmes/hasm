using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hasm.Parsing.Properties;
using NLog;
using OfficeOpenXml;
using ParserLib;
using ParserLib.Evaluation;
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

		public Rule FindRule(string input)
		{
			var operand = HasmGrammar.Operand.FirstValue(input);
			var instruction = _instructions.First(i => i.Grammar.StartsWith(operand.ToUpper()));

			var rule = _grammar.ParseInstruction(instruction);
			_logger.Debug(() => $"{input} - Found rule:{Environment.NewLine}{rule.PrettyFormat()}");

			return rule;
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