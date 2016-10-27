using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hasm.Properties;
using NLog;
using OfficeOpenXml;
using ParserLib;
using ParserLib.Evaluation;
using ParserLib.Parsing.Rules;

namespace hasm
{
	public class HasmParser
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly HasmGrammer _grammar;
		private readonly IList<Instruction> _instructions;

		public HasmParser(IDictionary<string, OperandType> defines)
		{
			_logger.Info($"{defines.Count} definitions");
			foreach (var pair in defines)
				_logger.Debug($"{pair.Key}: {pair.Value}");

			_grammar = new HasmGrammer(defines);
			_instructions = ParseInstructions().ToList(); // parseInstructions is deferred
			_logger.Info($"hasm knows {_instructions.Count} instructions");
		}

		public Rule FindRule(string input)
		{
			var operand = HasmGrammer.Operand.FirstValue(input);
			var instruction = _instructions.First(i => i.Grammar.StartsWith(operand.ToUpper()));

			var rule = _grammar.ParseInstruction(instruction);
			_logger.Debug(() => $"{input} - Found rule:{Environment.NewLine}{rule.PrettyFormat()}");

			return rule;
		}

		private static IEnumerable<Instruction> ParseInstructions()
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
						var instruction = Instruction.Parse(range);

						_logger.Debug($"Added: {instruction}");
						yield return instruction;
					}
				}
			}
		}
	}
}