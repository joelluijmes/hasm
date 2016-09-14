using System;
using System.Text.RegularExpressions;
using MicParser;
using ParserLib;
using ParserLib.Parsing.Value;

namespace MicAssembler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var statement = MicroGrammar._label;

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
                foreach (var p in parsed)
                {
                    Console.WriteLine(p.PrettyFormat());
                    Console.WriteLine();

                    //ValueNode<string> valueNode;
                    //if ((valueNode = p as ValueNode<string>) != null)
                    //    Console.WriteLine($"Value: {valueNode.Value}");
                }
            }
        }
    }
}