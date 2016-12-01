using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Fclp;
using hasm.Parsing.DependencyInjection;
using hasm.Parsing.Export;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using hasm.Parsing.Providers.SheetParser;
using NLog;
using ParserLib.Evaluation;

namespace hasm
{
	internal class Program
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private static IList<MicroFunction> _microFunctions;

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
            var microParser = new MicroFunctionSheetProvider();
            _microFunctions = microParser.Items;

            await LiveMode();

      //      if (Debugging)
		    //{
		    //    var consoleRule = LogManager.Configuration.LoggingRules.First(r => r.Targets.Any(t => t.Name == "console"));
		    //    consoleRule.EnableLoggingForLevel(LogLevel.Debug);

      //         await LiveMode();
		    //}
		    //else
		    //{
      //          var commandParser = new FluentCommandLineParser<ApplicationArguments>();
      //          commandParser.Setup(a => a.LiveMode)
      //              .As('l', "live-mode")
      //              .WithDescription("Use live mode");
      //          commandParser.SetupHelp("?", "help")
      //              .WithHeader("Invalid usage: ")
      //              .Callback(c => Console.WriteLine(c));

      //          var result = commandParser.Parse(args);
      //          if (result.HasErrors || result.EmptyArgs)
      //          {
      //              commandParser.HelpOption.ShowHelp(commandParser.Options);
      //              return;
      //          }

      //          await HandleArguments(commandParser.Object);
      //      }

			

			//var rule = MicroHasmGrammar.Alu;
			//Console.WriteLine(rule.PrettyFormat());
			//Console.WriteLine();

			//var tree = rule.ParseTree("0xFF-DST");

			//Console.WriteLine(tree.PrettyFormat());
            
            //var microInstructions = MicroGenerator.GenerateMicroInstructions(microParser.Items);

            //var assembler = new MicroAssembler();
            //var assembled = assembler.Assemble(microInstructions);

            //using (var stream = File.Open("out.txt", FileMode.Create, FileAccess.Write))
            //{
            //	var exporter = new FormattedExporter(stream) {Base = 2, AppendToString = true};
            //	//var exporter = new IntelHexExporter(stream);
            //	await exporter.Export(assembled);
            //}
        }

        private static async Task HandleArguments(ApplicationArguments arguments)
        {
            if (arguments.LiveMode)
                await LiveMode();
        }

        private static async Task LiveMode()
        {
            using (var stream = Console.OpenStandardOutput())
            using (var exporter = new FormattedExporter(stream) { AppendToString = true, Base = 2 })
            //using (var exporter = new IntelHexExporter(stream))
            {
                exporter.Writer.AutoFlush = true;
                Console.SetOut(exporter.Writer);

                while (true)
                {
                    Console.Write("Enter instruction: ");
                    var input = Console.ReadLine();
                    var index = input.IndexOf(':');
                    var address = 0;
                    if (index != -1)
                    {
                        address = int.Parse(input.Substring(0, index));
                        input = input.Substring(index + 1).Trim();
                    }

                    var opcode = HasmGrammar.Opcode.FirstValue(input).ToLower();
                    var function = _microFunctions.First(m => m.Instruction.ToLower().StartsWith(opcode));

                    var operands = HasmGrammar.GetOperands(function.Instruction)
                                              .Zip(HasmGrammar.GetOperands(input), (type, operand) => new MicroGenerator.Operand(type, operand));

                    MicroGenerator.PermuteFunction(operands, function);
                    var assembler = KernelFactory.Resolve<MicroAssembler>();
                    var assembled = assembler.Assemble(new[] {function}, address);

                    await exporter.Export(assembled);

                    Console.WriteLine();
                }
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
        private class ApplicationArguments
        {
            public bool LiveMode { get; set; }
        }
    }
}
