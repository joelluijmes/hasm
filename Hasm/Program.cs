using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Fclp;
using hasm.Parsing;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using hasm.Parsing.Parsers.Sheet;
using hasm.Properties;
using NLog;

namespace hasm
{
    internal class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static HasmSheetParser _hasmSheetParser;

        private static bool Debugging =>
#if DEBUG
            Debugger.IsAttached;
#else
			false;
#endif

        private static void Main(string[] args)
        {
#if DEBUG
            MainImpl(args);
#else
			try
			{
				AppDomain.CurrentDomain.UnhandledException += UnhandledException;
				MainImpl(args);
			}
			catch (Exception e)
			{
				if (Debugging)
					throw;

				UnhandledException(e);
			}
#endif
        }

        private static void MainImpl(string[] args)
        {
            if (Debugging)
            {
                var consoleRule = LogManager.Configuration.LoggingRules.First(r => r.Targets.Any(t => t.Name == "console"));
                consoleRule.EnableLoggingForLevel(LogLevel.Debug);

                Init();

					LiveMode();

                var listing = new List<string>();
                using (var memoryStream = new MemoryStream(Resources.listing))
                {
                    using (var reader = new StreamReader(memoryStream))
                    {
                        while (!reader.EndOfStream)
                            listing.Add(reader.ReadLine());
                    }
                }

                var assembler = new HasmAssembler(listing);
                var assembled = assembler.Process();

                var encoded = assembled.Aggregate("", (a, b) => $"{a} {b:X2}");
                File.WriteAllText("test.bin", encoded);
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

                Init();
                HandleArguments(commandParser.Object);
            }
        }

        private static void Init()
        { 
            var grammar = new HasmGrammar();
            _hasmSheetParser = new HasmSheetParser(grammar);
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

                var encoded = _hasmSheetParser.Encode(line);
                var value = ConvertToInt(encoded);

                _logger.Info($"Parsed {line} to encoding {Convert.ToString(value, 2).PadLeft(16, '0')}");

                Console.WriteLine();
            }
        }

        private static void AssembleFile(string input, string output)
        {
            var listing = File.ReadAllLines(input);
            var assembler = new HasmAssembler(listing);
            var assembled = assembler.Process();

            var encoded = assembled.Aggregate("", (a, b) => $"{a} {b:X2}");
            File.WriteAllText(output, encoded);
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
            Environment.Exit(-1);
        }

        private static int ConvertToInt(byte[] array)
        {
            var result = 0;
            for (var i = 0; i < array.Length; i++)
                result |= array[i] << (i * 8);

            return result;
        }

        private static void IfDebugging(Action action)
        {
#if DEBUG
            action();
#endif
        }

        private class ApplicationArguments
        {
            public string InputFile { get; set; }
            public string OutputFile { get; set; }
            public bool LiveMode { get; set; }
        }

    }
}