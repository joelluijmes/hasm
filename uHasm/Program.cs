using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using hasm.Parsing.Export;
using hasm.Parsing.Parsers.Sheet;
using NLog;

namespace hasm
{
	internal class Program
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private static bool Debugging =>
#if DEBUG
			Debugger.IsAttached;
#else
			false;
#endif

		private static void Main(string[] args)
		{
#if DEBUG
			MainImpl(args).Wait();
#else
			try
			{
				AppDomain.CurrentDomain.UnhandledException += UnhandledException;
				MainImpl(args).Wait();
			}
			catch (Exception e)
			{
				if (Debugging)
					throw;

				UnhandledException(e);
			}
#endif
		}

		private static async Task MainImpl(string[] args)
		{
			if (Debugging)
			{
				var consoleRule = LogManager.Configuration.LoggingRules.First(r => r.Targets.Any(t => t.Name == "console"));
				consoleRule.EnableLoggingForLevel(LogLevel.Debug);
			}

			//var rule = MicroHasmGrammar.Alu;
			//Console.WriteLine(rule.PrettyFormat());
			//Console.WriteLine();

			//var tree = rule.ParseTree("0xFF-DST");

			//Console.WriteLine(tree.PrettyFormat());

			var microParser = new MicroFunctionSheetParser();
			var microInstructions = MicroGenerator.GenerateMicroInstructions(microParser.Items);

			var assembler = new MicroAssembler();
			var assembled = assembler.Assemble(microInstructions);

			using (var stream = File.Open("out.txt", FileMode.Create, FileAccess.Write))
			{
				var exporter = new FormattedExporter(stream) {Base = 2, AppendToString = true};
			    //var exporter = new IntelHexExporter(stream);
				await exporter.Export(assembled);
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
			else
				UnhandledException(exception);
		}

		private static void UnhandledException(Exception e)
		{
			_logger.Fatal(e, "Unhandled exception");
			Environment.Exit(-1);
		}

		private static void IfDebugging(Action action)
		{
#if DEBUG
			action();
#endif
		}
	}
}
