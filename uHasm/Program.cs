using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fclp;
using hasm.Parsing.DependencyInjection;
using hasm.Parsing.Export;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using hasm.Parsing.Providers.SheetParser;
using NLog;
using ParserLib.Evaluation;
using ParserLib.Parsing;

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
            if (Debugging)
            {
                var consoleRule = LogManager.Configuration.LoggingRules.First(r => r.Targets.Any(t => t.Name == "console"));
                consoleRule.EnableLoggingForLevel(LogLevel.Debug);
            }
            
            var commandParser = new FluentCommandLineParser<ApplicationArguments>();
            commandParser.Setup(a => a.OutputFile)
                .As('o', "output")
                .WithDescription("Output file of the micro-assembler");
            commandParser.Setup(a => a.InputInstructionFile)
                .As("instruction")
                .WithDescription("\tIf set overrides default Instructionset sheet with given");
            commandParser.Setup(a => a.ExportDefaultInstructionset)
                .As('e', "export")
                .WithDescription("Export default Instructionset (saved as InstructionSet-default.xlsx)");
            commandParser.Setup(a => a.LiveMode)
                .As('l', "live-mode")
                .WithDescription("Use live mode");
            commandParser.Setup(a => a.GenerateGaps)
                .As('g', "generate-gaps")
                .WithDescription("Fills the gaps between instructions");
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

            var microParser = new MicroFunctionSheetProvider();
            _microFunctions = microParser.Items;

            if (arguments.LiveMode)
                await LiveMode();
            
            if (!string.IsNullOrEmpty(arguments.OutputFile))
                await AssembleFile(arguments.OutputFile, arguments.GenerateGaps);
            
        }

        private static async Task AssembleFile(string output, bool generateGaps = false)
        {
            var microInstructions = MicroGenerator.GenerateMicroInstructions(_microFunctions);

            var assembler = KernelFactory.Resolve<MicroAssembler>();
            var assembled = generateGaps
                ? MicroGenerator.GenerateGaps(assembler.Assemble(microInstructions)).ToArray()
                : assembler.Assemble(microInstructions).ToArray();

            using (var stream = File.Open($"{output}_format.txt", FileMode.Create, FileAccess.Write))
            {
                using (var exporter = new FormattedExporter(stream) { Base = 2, AppendToString = true })
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

        private static async Task LiveMode()
        {
            var assembler = KernelFactory.Resolve<MicroAssembler>();

            using (var stream = Console.OpenStandardOutput())
            {
                using (var exporter = new FormattedExporter(stream) {AppendToString = true, Base = 2})
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

                        MicroFunction function;
                        MicroInstruction instruction;
                        IEnumerable<IAssembled> assembled;

                        if (TryParseInstruction(input, out function))
                        {
                            assembled = assembler.Assemble(new[] {function}, address);
                            await exporter.Export(assembled);
                        }
                        else
                        {
                            if (TryParseMicroInstruction(input, out instruction))
                            {
                                assembled = new[] {assembler.Assemble(instruction)};
                                await exporter.Export(assembled);
                            }
                            else
                                Console.WriteLine("Unable to parse input.");
                        }

                        Console.WriteLine();
                    }
                }
            }
        }

        private static bool TryParseMicroInstruction(string input, out MicroInstruction instruction)
        {
            //instruction = null;

            var memory = Grammar.MatchChar(';') + Grammar.EnumValue<MemoryOperation>("memory");
            var next = Grammar.MatchChar(';') + Grammar.Node("next", Grammar.MatchString("NEXT"));
            var statusEnabled = Grammar.MatchChar(';') + Grammar.Node("status", Grammar.MatchString("STATUS"));
            var address = Grammar.MatchChar(';') + Grammar.Int32("addr");

            var rule = MicroHasmGrammar.Operation + memory.Optional + next.Optional + statusEnabled.Optional + address.Optional + Grammar.MatchChar(';').Optional;

            input = Regex.Replace(input, @"\s+", "").ToUpper();
            var tree = rule.ParseTree(input);
            var alu = tree.FirstValueOrDefault<Operation>() ?? Operation.NOP;

            var memoryOperation = tree.FirstValueByNameOrDefault<MemoryOperation>("memory");
            var last = tree.FirstNodeByNameOrDefault("next") != null;
            var status = tree.FirstNodeByNameOrDefault("status") != null;
            var addr = tree.FirstValueByNameOrDefault<int>("addr");

            instruction = new MicroInstruction(alu, memoryOperation, last, status);

            var nextInstruction = MicroInstruction.NOP;
            nextInstruction.Location = addr << 6;
            instruction.NextMicroInstruction = nextInstruction;

            return true;
        }

        private static bool TryParseInstruction(string input, out MicroFunction function)
        {
            function = null;
            var opcode = HasmGrammar.Opcode.FirstValueOrDefault(input);
            if (string.IsNullOrEmpty(opcode))
                return false;

            function = _microFunctions.FirstOrDefault(m => m.Instruction.ToLower().StartsWith(opcode.ToLower()));
            if (function == null)
                return false;

            var operands = HasmGrammar.GetOperands(function.Instruction).Zip(HasmGrammar.GetOperands(input), (type, operand) => new MicroGenerator.Operand(type, operand));
            function = MicroGenerator.PermuteFunction(operands, function);
            return true;
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
            public string InputInstructionFile { get; set; }
            public string OutputFile { get; set;  }
            public bool LiveMode { get; set;  }
            public bool GenerateGaps { get; set; }
            public bool ExportDefaultInstructionset { get; set; }
        }
    }
}
