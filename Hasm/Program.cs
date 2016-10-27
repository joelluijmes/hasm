using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Fclp;
using hasm.Parsing;
using NLog;
using ParserLib.Evaluation;

namespace hasm
{
	internal class Program
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private static readonly Dictionary<string, OperandType> _defines = new Dictionary<string, OperandType>
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
		
		private static void Main(string[] args)
		{
			try
			{
				AppDomain.CurrentDomain.UnhandledException += UnhandledException;

				if (Debugging)
				{
					var consoleRule = LogManager.Configuration.LoggingRules.First(r => r.Targets.Any(t => t.Name == "console"));
					consoleRule.EnableLoggingForLevel(LogLevel.Debug);
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

					var arguments = commandParser.Object;
					if (arguments.LiveMode)
						MenuLiveMode();
				}
				
				MenuLiveMode();
			}
			catch (Exception e)
			{
				UnhandledException(e);
			}
		}

		private static void MenuLiveMode()
		{
			var parser = new HasmParser(_defines);

			while (true)
			{
				Console.Write("Enter instruction: ");
				var line = Console.ReadLine();
				if (string.IsNullOrEmpty(line))
					break;

				var rule = parser.FindRule(line);

				var encoded = rule.FirstValue<int>(line);
				_logger.Info($"Parsed {line} to encoding {encoded:X3}");

				Console.WriteLine();
			}
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
