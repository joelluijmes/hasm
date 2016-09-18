using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Fclp;
using MicParser;
using MicParser.OpCode;
using ParserLib;
using ParserLib.Evaluation;
using ParserLib.Parsing.Rules;

namespace MicAssembler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var commandParser = new FluentCommandLineParser<ApplicationArguments>();
            commandParser.Setup(a => a.InputFile)
                .As('i', "input")
                .WithDescription("\tInput file to be assembled");
            commandParser.Setup(a => a.OutputFile)
                .As('o', "output")
                .WithDescription("Output file of the assembler");
            commandParser.Setup(a => a.ShowGrammar)
                .As('s', "show-grammar")
                .WithDescription("Print the used grammar tree");
            commandParser.Setup(a => a.LiveMode)
                .As('l', "live-mode")
                .WithDescription("Use live mode");
            commandParser.SetupHelp("?", "help")
                .WithHeader("Invalid usage: ")
                .Callback(c => Console.WriteLine(c));

            var result = commandParser.Parse(args);
            if (result.HasErrors)
            {
                commandParser.HelpOption.ShowHelp(commandParser.Options);
                return;
            }
            
            var statement = MicroGrammar.Instruction;
            var arguments = commandParser.Object;

            if (arguments.ShowGrammar)
                PrintGrammar(statement);
            
            if (arguments.LiveMode && string.IsNullOrEmpty(arguments.InputFile) && string.IsNullOrEmpty(arguments.OutputFile))
            {
                LiveMode(statement, arguments.ShowGrammar);
                return;
            }

            if (!string.IsNullOrEmpty(arguments.InputFile) && !string.IsNullOrEmpty(arguments.OutputFile))
            {
                var lookup = new Dictionary<string, int>
                {
                    ["main"] = 0,
                    ["iadd1"] = 0x60,
                };

                var lines = File.ReadAllLines(arguments.InputFile);
                var assembler = new Assembler(statement, lookup);
                var instructions = assembler.Parse(lines);

                SaveInstructions(instructions, arguments.OutputFile);
            }
            else
            {
                commandParser.HelpOption.ShowHelp(commandParser.Options);
            }
        }

        private static void SaveInstructions(IEnumerable<MicroInstruction> listing, string path)
        {
            
        }

        private static void PrintGrammar(Rule grammar)
        {
            Console.WriteLine($"Current grammar: {grammar} -> {grammar.Definition}");
            Console.WriteLine(grammar.PrettyFormat());
        }

        private static void LiveMode(Rule statement, bool printTree)
        {
            Console.WriteLine("Enter expression to parse. Use exit to stop.");

            while (true)
            {
                Console.WriteLine();
                Console.Write("Input: ");
                var input = Console.ReadLine();
                if (input == "exit")
                    return;

                input = Regex.Replace(input, "\\s+", "");

                var parsed = statement.ParseTree(input);
                Console.WriteLine("=== [Result]");

                if (printTree)
                    Console.WriteLine(parsed.PrettyFormat());

                var opcode = parsed.FirstValueOrDefault<long>();
                Console.WriteLine($"opcode: {Regex.Replace($"{opcode:X9}", ".{3}", "$0 ")}");
            }
        }

        private class ApplicationArguments
        {
            public string InputFile { get; set; }

            public string OutputFile { get; set; }

            public bool ShowGrammar { get; set; }

            public bool LiveMode { get; set; }
        }
    }
}