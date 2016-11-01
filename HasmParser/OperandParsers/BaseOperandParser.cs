using System;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using NLog;
using ParserLib.Evaluation;
using ParserLib.Evaluation.Rules;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.OperandParsers
{
	/// <summary>
	/// Base class for predefined parsers
	/// </summary>
	/// <seealso cref="IOperandParser" />
	public abstract class BaseOperandParser : IOperandParser
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly ValueRule<string> _encodingMask;

		/// <summary>
		/// The mask of this parser in the encoding
		/// </summary>
		protected readonly char Mask;

		/// <summary>
		/// The name of the parser
		/// </summary>
		protected readonly string Name;

		/// <summary>
		/// The amount of bits it take in the encoding.
		/// </summary>
		protected readonly int Size;

		private Rule _rule;

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseOperandParser"/> class.
		/// </summary>
		/// <param name="name">The name of rule.</param>
		/// <param name="mask">The mask in the encoding.</param>
		/// <param name="size">The amount of bits in the encoding.</param>
		protected BaseOperandParser(string name, char mask, int size)
		{
			Name = name;
			Mask = mask;
			Size = size;
			_encodingMask = HasmGrammar.CreateMaskRule(mask);
		}

		/// <summary>
		/// Gets the type of the operand.
		/// </summary>
		/// <value>
		/// The type of the operand.
		/// </value>
		public abstract OperandType OperandType { get; }

		/// <summary>
		/// Creates the rule for this parser.
		/// </summary>
		/// <param name="encoding">The encoding.</param>
		/// <returns>
		/// Rule which fills in the encoding
		/// </returns>
		public Rule CreateRule(string encoding)
		{
			if (_rule != null)
				return _rule;

			var matchRule = Grammar.FirstValue<string>(CreateMatchRule());
			Func<string, int> converter = match =>
			{
				var value = matchRule.FirstValue(match);
				return Encode(encoding, value);
			};

			_rule = Grammar.ConvertToValue(Name, converter, matchRule);
			_logger.Debug($"Created parser for {Name}");

			return _rule;
		}

		/// <summary>
		/// Creates the match rule for the grammar.
		/// </summary>
		/// <returns>The rule.</returns>
		protected abstract Rule CreateMatchRule();

		/// <summary>
		/// Converts number to binary string
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>Converted binary string.</returns>
		protected string NumberConverter(string value)
		{
			var number = int.Parse(value);
			var binary = Convert.ToString(number, 2).PadLeft(Size, '0');
			return binary.Substring(0, Math.Min(Size, binary.Length));
		}

		private int Encode(string encoding, string value)
		{
			var opcodeBinary = _encodingMask.FirstValue(encoding); // gets the binary representation of the encoding
			var index = opcodeBinary.IndexOf(Mask); // finds the first occurance of the mask
			var nextIndex = opcodeBinary.IndexOf('0', index); // and the last
			if (nextIndex == -1)
				nextIndex = opcodeBinary.Length; // could be that it ended with the mask so we set it to the length of total encoding

			var length = nextIndex - index;
			opcodeBinary = opcodeBinary.Remove(index, length).Insert(index, value);
			var result = Convert.ToInt32(opcodeBinary, 2);

			return result;
		}
	}
}