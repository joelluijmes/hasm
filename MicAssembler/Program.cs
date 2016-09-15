using System;
using System.IO;
using System.Text.RegularExpressions;
using MicParser;
using MicParser.OpCode;
using ParserLib;
using ParserLib.Evaluation;

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
            //Console.WriteLine();

            //while (true)
            //{
            //    Console.WriteLine("Input: ");
            //    var input = Console.ReadLine();
            //    input = Regex.Replace(input, "\\s+", "");

            //    var parsed = statement.ParseTree(input);

            //    Console.WriteLine("=== [Result]");
            //    Console.WriteLine(parsed.PrettyFormat());
            //    Console.WriteLine($"Value: {Evaluator.FirstValueOrDefault<MicroInstruction>(parsed)}");
            //    //Console.WriteLine($"Value: {Evaluator.FirstValueOrDefault<string>(parsed)}");
            //}

            var lines = File.ReadAllLines("assembly.txt");
            foreach (var line in lines)
            {
                var parsed = statement.ParseTree(Regex.Replace(line, "\\s+", ""));
                var instruction = Evaluator.FirstValue<MicroInstruction>(parsed);

                Console.WriteLine(instruction);
            }
        }
    }
}