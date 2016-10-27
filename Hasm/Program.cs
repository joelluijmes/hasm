using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hasm.Parsing;
using NLog;
using ParserLib.Evaluation;

namespace hasm
{
	internal class Program
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private static void Main(string[] args)
		{
			try
			{
				AppDomain.CurrentDomain.UnhandledException += UnhandledException;

				ExecuteIfDebug(() =>
				{
					var consoleRule = LogManager.Configuration.LoggingRules.First(r => r.Targets.Any(t => t.Name == "console"));
					consoleRule.EnableLoggingForLevel(LogLevel.Debug);
				});

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

				var parser = new HasmParser(defines);

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
			catch (Exception e)
			{
				UnhandledException(e);
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

		private static void ExecuteIfDebug(Action action)
		{
#if DEBUG
			if (Debugger.IsAttached)
				action();
#endif
		}
	}
}
