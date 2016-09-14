﻿using System;
using System.Text.RegularExpressions;
using MicParser;
using ParserLib;
using ParserLib.Evaluation;

namespace MicAssembler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var statement = MicroGrammar.Statement;

            Console.WriteLine($"Current grammar: {statement} -> {statement.Definition}");
            Console.WriteLine("=== [ParseTree]");
            Console.WriteLine(statement.PrettyFormat());
            Console.WriteLine();

            while (true)
            {
                Console.WriteLine("Input: ");
                var input = Console.ReadLine();
                input = Regex.Replace(input, "\\s+", "");

                var parsed = statement.ParseTree(input);

                Console.WriteLine("=== [Result]");
                Console.WriteLine(parsed.PrettyFormat());
                Console.WriteLine($"Value: {Evaluator.FirstValueOrDefault<long>(parsed)}");
            }
        }
    }
}