using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fclp;
using hasm.Parsing.DependencyInjection;
using hasm.Parsing.Encoding;
using hasm.Parsing.Export;
using hasm.Parsing.Providers.SheetParser;
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

            var commandParser = new FluentCommandLineParser<ApplicationArguments>();
            commandParser.Setup(a => a.InputFile)
                .As('i', "input")
                .WithDescription("\tInput file to be assembled");
            commandParser.Setup(a => a.InputInstructionFile)
                .As("instruction")
                .WithDescription("\tIf set overrides default Instructionset sheet with given");
            commandParser.Setup(a => a.OutputFile)
                .As('o', "output")
                .WithDescription("Output file of the assembler");
            commandParser.Setup(a => a.ExportDefaultInstructionset)
                .As('e', "export")
                .WithDescription("Export default Instructionset (saved as InstructionSet-default.xlsx)");
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

            await HandleArguments(commandParser.Object);
        }

        private static async Task HandleArguments(ApplicationArguments arguments)
        {
            if (arguments.ExportDefaultInstructionset)
                File.WriteAllBytes("Instructionset-default.xlsx", BaseSheetProvider.Instructionset);

            if (!string.IsNullOrEmpty(arguments.InputInstructionFile))
                BaseSheetProvider.Instructionset = File.ReadAllBytes(arguments.InputInstructionFile);

            if (arguments.LiveMode)
                LiveMode();
            else
            {
                if (string.IsNullOrEmpty(arguments.InputFile) || string.IsNullOrEmpty(arguments.OutputFile))
                    throw new InvalidOperationException("If you haven't chosen for live mode you want to assemble a listing. Therefor you need to give the input- and output file.");
            }

            await AssembleFile(arguments.InputFile, arguments.OutputFile);
        }

        private static void LiveMode()
        {
            var encoder = KernelFactory.Resolve<HasmEncoder>();

            while (true)
            {
                Console.Write("Enter instruction: ");
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                var encoded = encoder.Encode(line);
                var value = ConvertToInt(encoded);

                _logger.Info($"Parsed {line} to encoding {Convert.ToString(value, 2).PadLeft(16, '0')}");

                Console.WriteLine();
            }
        }

        private static async Task AssembleFile(string input, string output)
        {
            var listing = File.ReadAllLines(input);
            var assembler = KernelFactory.Resolve<HasmAssembler>();
            var assembled = assembler.Process(listing).ToArray();

            using (var stream = File.Open($"{output}_format.txt", FileMode.Create, FileAccess.Write))
            {
                using (var exporter = new FormattedExporter(stream) {Base = 2, AppendToString = true})
                    await exporter.Export(assembled);
            }

            using (var stream = File.Open($"{output}_intel.txt", FileMode.Create, FileAccess.Write))
            {
                using (var exporter = new IntelHexExporter(stream))
                    await exporter.Export(assembled);
            }

            using (var stream = File.Open($"{output}_binary.txt", FileMode.Create, FileAccess.Write))
            {
                using (var exporter = new BinaryExporter(stream))
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

        private static int ConvertToInt(byte[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            var result = 0;
            for (var i = 0; i < array.Length; i++)
                result |= array[i] << (i*8);

            return result;
        }

        private class ApplicationArguments
        {
            public string InputInstructionFile { get; set; }
            public string InputFile { get; set; }
            public string OutputFile { get; set; }
            public bool LiveMode { get; set; }
            public bool ExportDefaultInstructionset { get; set; }
        }
    }
}
