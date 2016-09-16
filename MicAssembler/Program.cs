using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MicParser;
using MicParser.OpCode;
using ParserLib;
using ParserLib.Evaluation;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace MicAssembler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var statement = MicroGrammar.Instruction;

            //Console.WriteLine($"Current grammar: {statement} -> {statement.Definition}");
            //Console.WriteLine("=== [ParseTree]");
            //Console.WriteLine(statement.PrettyFormat());
            
            //while (true)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine("Input: ");
            //    var input = Console.ReadLine();
            //    input = Regex.Replace(input, "\\s+", "");

            //    var parsed = statement.ParseTree(input);
            //    Console.WriteLine("=== [Result]");
            //    Console.WriteLine(parsed.PrettyFormat());

            //    var opcode = Evaluator.FirstValueOrDefault<long>(parsed);

            //    Console.WriteLine($"opcode: {Regex.Replace($"{opcode:X9}", ".{3}", "$0 ")}");
            //    //Console.WriteLine($"Value: {Evaluator.FirstValueOrDefault<string>(parsed)}");
            //}

            var lookup = new Dictionary<string, byte>
            {
                ["main"] = 0,
                ["iadd1"] = 0x60,
            };

            var lines = File.ReadAllLines("assembly.txt");
            //var instructions = lines
            //    .Select(line => statement.ParseTree(Regex.Replace(line, "\\s+", "")))
            //    .Select(parsed => parsed.Value<MicroInstruction>())
            //    .ToDictionary(instr => instr.Label);

            //foreach (var labelInstruction in instructions)
            //{
            //    var instruction = labelInstruction.Value;
            //    Console.WriteLine(instruction);

            //    byte address;

            //    if (lookup.TryGetValue(labelInstruction.Key, out address))
            //        instruction.Address = address;

            //    if (!lookup.TryGetValue(instruction.Branch, out address))
            //        continue;

            //    instruction.OpCode.NextAddress = address;
            //    instruction.Branch = "";
            //}

            //Console.WriteLine();

            var assembler = new Assembler(statement);
            var instructions = assembler.Parse(lines);

            foreach (var instruction in instructions)
                Console.WriteLine(instruction);

        }
    }

    class Assembler
    {
        private static readonly Rule _labelRule = ValueGrammar.Text("name", SharedGrammar.Letters) + ValueGrammar.ConvertToValue("index", int.Parse, SharedGrammar.Digits).Optional;

        public Assembler(Rule grammar)
        {
            Grammar = grammar;
        }

        public Rule Grammar { get; }

        public IList<MicroInstruction> Parse(IEnumerable<string> listing)
        {
            IList<MicroInstruction> parsedInstructions = listing
                .Select(l => Regex.Replace(l, "\\s+", "")) // remove whitespace
                .Select(Grammar.ParseTree) // parse every line
                .Select(p => p.Value<MicroInstruction>()) // get the MicroInstruction 
                .ToList();

            parsedInstructions = FixLabels(parsedInstructions);
            parsedInstructions = FixBranch(parsedInstructions);

            return parsedInstructions;
        }

        private static IList<MicroInstruction> FixLabels(IList<MicroInstruction> listing)
        {
            var previous = listing.First();
            if (string.IsNullOrEmpty(previous.Label))
                throw new ArgumentException("Invalid listing: first instruction doesn't have a label");

            foreach (var instruction in listing.Skip(1))
            {
                if (string.IsNullOrEmpty(instruction.Label))
                {
                    var parsed = _labelRule.ParseTree(previous.Label);
                    var name = parsed.Value<string>();
                    var index = Evaluator.FirstValueOrDefault<int>(parsed);

                    instruction.Label = $"{name}{++index}";
                }

                previous = instruction;
            }

            return listing;
        }

        private static IList<MicroInstruction> FixBranch(IList<MicroInstruction> listing)
        {
            for (var i = 0; i < listing.Count; i++)
            {
                if (i == listing.Count - 1)
                    break;

                var current = listing[i];
                var next = listing[i + 1];

                if (string.IsNullOrEmpty(current.Branch))
                    current.Branch = next.Label;
            }

            if (string.IsNullOrEmpty(listing.Last().Branch))
                throw new ArgumentException("Invalid listing: last instruction doesn't have a branch");

            return listing;
        }

        private enum InstructionState
        {
            
        }
    }
}