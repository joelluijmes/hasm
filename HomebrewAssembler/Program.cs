using System;
using System.Text.RegularExpressions;
using HomebrewParser;
using ParserLib.Parsing.Value;
using ParserLib;

namespace HomebrewAssembler
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
                Console.Write("Input: ");
                var input = Console.ReadLine();
                input = Regex.Replace(input, "\\s+", "");

                var parsed = statement.ParseTree(input);

                Console.WriteLine("=== [Result]");
                foreach (var p in parsed)
                {
                    Console.WriteLine(p.PrettyFormat());
                    Console.WriteLine();

                    ValueNode<long> valueNode;
                    if ((valueNode = p as ValueNode<long>) != null)
                        Console.WriteLine($"Value: {valueNode.Value:X9}");
                }
            }
        }
    }
}