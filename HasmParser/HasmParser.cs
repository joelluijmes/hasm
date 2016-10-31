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
	/// <summary>
	/// This class makes it possible to parse an instruction to an encoding as specified
	/// in the HasmGrammar.
	/// </summary>
	public class HasmParser
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly HasmGrammar _grammar;
		private readonly IList<InstructionEncoding> _instructions;
		private readonly Dictionary<string, ValueRule<byte[]>> _rules;

		/// <summary>
		/// Initializes a new instance of the <see cref="HasmParser"/> class.
		/// </summary>
		/// <param name="grammar">The grammar of the parser.</param>
		public HasmParser(HasmGrammar grammar)
		{
			foreach (var pair in grammar.Definitions)
				_logger.Debug($"{pair.Key}: {pair.Value}");

			_grammar = grammar;
		    _instructions = ParseInstructions();
			_rules = new Dictionary<string, ValueRule<byte[]>>();
			_logger.Info($"Learned {_instructions.Count} instructions");
		}

		/// <summary>
		/// Encodes the specified input.
		/// </summary>
		/// <param name="input">The instruction to be parsed.</param>
		/// <returns>Parsed instruction</returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public byte[] Encode(string input)
		{
			byte[] encoded;
			if (!TryEncode(input, out encoded))
				throw new NotImplementedException();

			return encoded;
		}

		/// <summary>
		/// Tries to encode the input, if partly-failed (due label) the encoded will still be created
		/// as an array of the expected length.
		/// </summary>
		/// <param name="input">The input to be encoded.</param>
		/// <param name="encoded">The encoded instruction.</param>
		/// <returns>True if succeeds</returns>
		/// <exception cref="System.ArgumentNullException">input</exception>
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

		private static IList<InstructionEncoding> ParseInstructions()
		{
			var encoding = new List<InstructionEncoding>();

			using (var stream = new MemoryStream(Resources.Instructionset))
			using (var package = new ExcelPackage(stream))
			{
				var sheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name == "Encoding");
				if (sheet == null)
					throw new NotImplementedException();

				var start = sheet.Dimension.Start;
				var end = sheet.Dimension.End;

				for (var row = start.Row + 1; row <= end.Row; ++row)
				{
					var range = sheet.Cells[row, 1, row, end.Column];
					var instruction = InstructionEncoding.Parse(range);

					_logger.Debug($"Added: {instruction}");
					encoding.Add(instruction);
				}
			}

			return encoding;
		}
	}
}