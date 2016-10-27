using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Fclp;
using hasm.Parsing;
using hasm.Properties;
using NLog;
using ParserLib.Evaluation;

namespace hasm
{
	internal class Program
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private static readonly HasmParser _hasmParser;

		static Program()
		{
			var defines = new Dictionary<string, OperandType>
			{
				["REG"] = OperandType.FirstGeneralRegister,
				["DST"] = OperandType.FirstGeneralRegister,
				["REG1"] = OperandType.FirstGeneralRegister,
				["REG2"] = OperandType.SecondGeneralRegister,
				["SRC"] = OperandType.SecondGeneralRegister,

				["SPC"] = OperandType.FirstSpecialRegister,
				["SPC1"] = OperandType.FirstSpecialRegister,
				["SPC2"] = OperandType.SecondSpecialRegister,

				["IMM6"] = OperandType.Immediate6,
				["IMM8"] = OperandType.Immediate8,
				["IMM12"] = OperandType.Immediate12,

				["c"] = OperandType.BranchIf,
				["PAIR + IMM6"] = OperandType.PairOffset
			};

			var grammar = new HasmGrammar(defines);
			_hasmParser = new HasmParser(grammar);
		}

		private static void Main(string[] args)
		{
			try
			{
				AppDomain.CurrentDomain.UnhandledException += UnhandledException;

				if (Debugging)
				{
					var consoleRule = LogManager.Configuration.LoggingRules.First(r => r.Targets.Any(t => t.Name == "console"));
					consoleRule.EnableLoggingForLevel(LogLevel.Debug);

					//LiveMode();

					var listing = new List<string>();
					using (var memoryStream = new MemoryStream(Resources.listing))
					using (var reader = new StreamReader(memoryStream))
					{
						while (!reader.EndOfStream)
							listing.Add(reader.ReadLine());
					}

					var assembler = new Assembler(_hasmParser, listing);
				}
				else
				{
					var commandParser = new FluentCommandLineParser<ApplicationArguments>();
					commandParser.Setup(a => a.InputFile)
						.As('i', "input")
						.WithDescription("\tInput file to be assembled");
					commandParser.Setup(a => a.OutputFile)
						.As('o', "output")
						.WithDescription("Output file of the assembler");
					commandParser.Setup(a => a.LiveMode)
						.As('l', "live-mode")
						.WithDescription("Use live mode");
					commandParser.SetupHelp("?", "help")
						.WithHeader("Invalid usage: ")
						.Callback(c => Console.WriteLine(c));

					var result = commandParser.Parse(args);
					if (result.HasErrors || result.EmptyArgs)
					{
						commandParser.HelpOption.ShowHelp(commandParser.Options);
						return;
					}

					HandleArguments(commandParser.Object);
				}
			}
			catch (Exception e)
			{
				UnhandledException(e);
			}
		}

		private static void HandleArguments(ApplicationArguments arguments)
		{
			if (arguments.LiveMode)
				LiveMode();
			else if (string.IsNullOrEmpty(arguments.InputFile) || string.IsNullOrEmpty(arguments.OutputFile))
				throw new InvalidOperationException("If you haven't chosen for live mode you want to assemble a listing. Therefor you need to give the input- and output file.");

			AssembleFile(arguments.InputFile, arguments.OutputFile);
		}

		private static void LiveMode()
		{
			while (true)
			{
				Console.Write("Enter instruction: ");
				var line = Console.ReadLine();
				if (string.IsNullOrEmpty(line))
					break;

				var rule = _hasmParser.FindRule(line);

				var encoded = rule.FirstValue<int>(line);
				_logger.Info($"Parsed {line} to encoding {encoded:X3}");

				Console.WriteLine();
			}
		}

		private static void AssembleFile(string input, string output)
		{
			var listing = File.ReadAllLines(input);
			var assembler = new Assembler(_hasmParser, listing);
			var assembled = assembler.Process();

			File.WriteAllBytes(output, assembled);
		}

		private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exception = e.ExceptionObject as Exception;
			if (exception == null)
			{
				_logger.Fatal($"ExceptionObject is not an exception: {e.ExceptionObject}");
				Environment.Exit(-1);
			}
			else UnhandledException(exception);
		}

		private static void UnhandledException(Exception e)
		{
			_logger.Fatal(e, "Unhandled exception");
			//ExecuteIfDebug(() => { throw e; });

			Environment.Exit(-1);
		}

		private static bool Debugging =>
#if DEBUG
			Debugger.IsAttached;
#else
			false;
#endif

		private class ApplicationArguments
		{
			public string InputFile { get; set; }
			public string OutputFile { get; set; }
			public bool LiveMode { get; set; }
		}
	}
}
