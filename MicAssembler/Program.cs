using System;
using System.Collections.Generic;
using System.IO;
using MicParser;

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

            var lookup = new Dictionary<string, int>
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

            var assembler = new Assembler(statement, lookup);
            var instructions = assembler.Parse(lines);

            foreach (var instruction in instructions)
                Console.WriteLine(instruction);
        }
    }
}