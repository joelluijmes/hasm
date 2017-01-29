using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fclp;
using hasm.Assembler;
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
                .As('s', "set")
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
            commandParser.Setup(a => a.OutputPreProcess)
                .As('p')
                .WithDescription("Export the files before alignment, useful for line by line assembly");
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

            await AssembleFile(arguments.InputFile, arguments.OutputFile, arguments.OutputPreProcess);
        }

        private static void LiveMode()
        {
            var encoder = KernelFactory.Resolve<HasmEncoder>();

            while (true)
            {
                Console.Write("Enter instruction: ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var encoded = encoder.Encode(line);
                var value = ConvertToInt(encoded);

                var storeAddress = 0;
                if (encoded.Length == 1)
                    storeAddress = value << 7;
                else if (encoded.Length == 2)
                    storeAddress = value >> 1;
                else if (encoded.Length == 3)
                    storeAddress = value >> 9;

                var spaceRegex = new Regex(".{4}");
                var binary = spaceRegex.Replace(Convert.ToString(value, 2).PadLeft(40, '0'), "$0 ").Trim();
                Console.WriteLine($"({storeAddress:X4}h): {value:X5}h {binary}");

                Console.WriteLine();
            }
        }

        private static async Task AssembleFile(string input, string output, bool outputPreAssembled = false)
        {
            var listing = File.ReadAllLines(input);
            var assembler = KernelFactory.Resolve<HasmAssembler>();
            var assembled = assembler.Process(listing).Select(a => new ReverseEndianAssembled(a)).ToArray();

            if (outputPreAssembled)
                await OutputAssembled(output + "_pre", assembled);
            
            await OutputAssembled(output, AlignAssmembled(assembled, HasmAssembler.WORDSIZE));
        }

        private static async Task OutputAssembled(string name, IAssembled[] assembled)
        {
            using (var stream = File.Open($"{name}_format.txt", FileMode.Create, FileAccess.Write))
            {
                using (var exporter = new FormattedExporter(stream) {Base = 2, AppendToString = true})
                    await exporter.Export(assembled);
            }

            using (var stream = File.Open($"{name}_intel.txt", FileMode.Create, FileAccess.Write))
            {
                using (var exporter = new IntelHexExporter(stream))
                    await exporter.Export(assembled);
            }

            using (var stream = File.Open($"{name}_binary.txt", FileMode.Create, FileAccess.Write))
            {
                using (var exporter = new BinaryExporter(stream))
                    await exporter.Export(assembled);
            }
        }

        private static IAssembled[] AlignAssmembled(IEnumerable<IAssembled> assembled, int size = 2)
        {
            var data = assembled.Select(a => a.Bytes).SelectMany(a => a).ToArray();
            var padding = data.Length%size;

            // add nops to the end
            if (padding != 0)
            {
                Array.Resize(ref data, data.Length + padding);
                for (var i = data.Length - 1; i < data.Length; ++i)
                    data[i] = 0xFF;
            }

            var aligned = new IAssembled[data.Length/size];
            for (var i = 0; i < data.Length/size; ++i)
            {
                var buf = data.Skip(i*size).Take(size).ToArray();
                aligned[i] = new RawAssembled(i, buf);
            }

            return aligned;
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
            public bool OutputPreProcess { get; set; }
        }
    }
}
