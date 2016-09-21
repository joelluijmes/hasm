﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Fclp;
using MicParser;
using ParserLib;
using ParserLib.Evaluation;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace MicMicroAssembler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var commandParser = new FluentCommandLineParser<ApplicationArguments>();
            commandParser.Setup(a => a.InputFile)
                .As('i', "input")
                .WithDescription("\tInput file to be (dis)assembled");
            commandParser.Setup(a => a.OutputFile)
                .As('o', "output")
                .WithDescription("Output file of the (dis)assembler");
            commandParser.Setup(a => a.ShowGrammar)
                .As('s', "show-grammar")
                .WithDescription("Print the used grammar tree (not appliable to disassembler)");
            commandParser.Setup(a => a.LiveMode)
                .As('l', "live-mode")
                .WithDescription("Use live mode");
            commandParser.Setup(a => a.ConstantsFile)
                .As('c', "constants")
                .WithDescription("Constants file, required to correctly assemble (not appliable to disassembler)");
            commandParser.Setup(a => a.Disassemble)
                .As('d', "disassemble")
                .WithDescription("Disassemble instead of assembling instructions");
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

#if DEBUG
            if (Debugger.IsAttached)
            {
                //arguments.ShowGrammar = true;
                //arguments.LiveMode = true;
                arguments.Disassemble = true;
                arguments.InputFile = "out.txt";
                arguments.OutputFile = "dis.txt";

                //arguments.InputFile = "mic-1.txt";
                //arguments.OutputFile = "out.txt";
                //arguments.ConstantsFile = "c_ijvm.txt";
            }
#endif

            if (arguments.ShowGrammar)
                PrintGrammar(statement);

            if (arguments.LiveMode && string.IsNullOrEmpty(arguments.InputFile) && string.IsNullOrEmpty(arguments.OutputFile) && string.IsNullOrEmpty(arguments.ConstantsFile))
            {
                LiveMode(statement, arguments);
                return;
            }

            if (!arguments.Disassemble && !string.IsNullOrEmpty(arguments.InputFile) && !string.IsNullOrEmpty(arguments.OutputFile) && !string.IsNullOrEmpty(arguments.ConstantsFile))
            {
                var lines = File.ReadAllLines(arguments.InputFile);
                var lookup = ParseConstants(arguments.ConstantsFile);
                var assembler = new Assembler(statement, lookup);
                var instructions = assembler.Parse(lines);

                SaveInstructions(instructions, arguments.OutputFile);
            }
            else if (arguments.Disassemble && !string.IsNullOrEmpty(arguments.InputFile) && !string.IsNullOrEmpty(arguments.OutputFile))
            {
                var lines = File.ReadAllLines(arguments.InputFile);
                var disassembler = new Disassembler();

                var instructions = disassembler.Parse(lines);
                File.WriteAllLines(arguments.OutputFile, instructions.Select(s => s.ToString()));
            }
            else
            {
                commandParser.HelpOption.ShowHelp(commandParser.Options);
            }
        }

        private static void SaveInstructions(IEnumerable<MicroInstruction> listing, string path)
        {
            using (var file = File.CreateText(path))
            {
                var ordered = listing.OrderBy(m => m.Address);

                MicroInstruction previous = null;
                foreach (var current in ordered)
                {
                    if (previous != null)
                        for (var j = 0; j < current.Address - previous.Address - 1; ++j)
                            file.WriteLine($"{Regex.Replace($"{1:X9}", ".{3}", "$0 ")}");

                    file.WriteLine($"{Regex.Replace($"{current.OpCode.Value:X9}", ".{3}", "$0 ")}");
                    previous = current;
                }
            }
        }

        private static void PrintGrammar(Rule grammar)
        {
            Console.WriteLine($"Current grammar: {grammar} -> {grammar.Definition}");
            Console.WriteLine(grammar.PrettyFormat());
        }

        private static void LiveMode(Rule statement, ApplicationArguments arguments)
        {
            Console.WriteLine(arguments.Disassemble 
                ? "Enter value to disassemble. Use exit to stop." 
                : "Enter expression to assemble. Use exit to stop.");

            while (true)
            {
                Console.WriteLine();
                Console.Write("Input: ");
                var input = Console.ReadLine();
                if (input == "exit")
                    return;

                input = Regex.Replace(input, "\\s+", "");

                if (arguments.Disassemble)
                {
                    var disassembler = new Disassembler();
                    var result = disassembler.Parse(new[] {input}).First();

                    Console.WriteLine(result);
                }
                else
                {
                    var parsed = statement.ParseTree(input);
                    Console.WriteLine("=== [Result]");

                    if (arguments.ShowGrammar)
                        Console.WriteLine(parsed.PrettyFormat());

                    var opcode = parsed.FirstValueOrDefault<long>();
                    Console.WriteLine($"opcode: {Regex.Replace($"{opcode:X9}", ".{3}", "$0 ")}");
                }
            }
        }

        private static Dictionary<string, int> ParseConstants(string constansFile)
        {
            var hex = ValueGrammar.FirstValue<int>("address", Grammar.MatchString("0x", true) + ValueGrammar.ConvertToValue("hex", s => Convert.ToInt32(s, 16), Grammar.OneOrMore(SharedGrammar.Hexadecimal)));
            var label = ValueGrammar.Text("label", MicroGrammar.Label);
            var rule = hex + Grammar.MatchChar(':') + label;

            var dict = new Dictionary<string, int>();
            foreach (var line in File.ReadAllLines(constansFile))
            {
                var tree = rule.ParseTree(Regex.Replace(line, "\\s+", ""));

                var key = tree.FirstValueByName<string>(label.Name);
                var value = tree.FirstValueByName<int>(hex.Name);

                dict[key] = value;
            }

            return dict;
        }

        private class ApplicationArguments
        {
            public string InputFile { get; set; }
            public string OutputFile { get; set; }
            public string ConstantsFile { get; set; }
            public bool ShowGrammar { get; set; }
            public bool LiveMode { get; set; }
            public bool Disassemble { get; set; }
        }
    }
}