using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MicParser.OpCode;
using ParserLib.Evaluation;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace MicAssembler
{
    internal class Assembler
    {
        private static readonly Rule _labelRule = ValueGrammar.Text("name", SharedGrammar.Letters) + ValueGrammar.ConvertToValue("index", int.Parse, SharedGrammar.Digits).Optional;

        public Assembler(Rule grammar, IDictionary<string, int> positions)
        {
            Grammar = grammar;
            Positions = positions;
        }

        public Rule Grammar { get; }
        public IDictionary<string, int> Positions { get; }

        public IList<MicroInstruction> Parse(IEnumerable<string> listing)
        {
            IList<MicroInstruction> parsedInstructions = listing
                .Select(l => Regex.Replace(l, "\\s+", "")) // remove whitespace
                .Select(Grammar.ParseTree) // parse every line
                .Select(p => p.FirstValue<MicroInstruction>()) // get the MicroInstruction 
                .ToList();

            parsedInstructions = FixLabels(parsedInstructions);
            parsedInstructions = FixBranch(parsedInstructions);
            parsedInstructions = FitInstructions(parsedInstructions, Positions);
            parsedInstructions = FixAddresses(parsedInstructions, Positions);

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
                    var name = parsed.FirstValue<string>();
                    var index = parsed.FirstValueOrDefault<int>();

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

                if (string.IsNullOrEmpty(current.Branch) && (current.OpCode.NextAddress == 0))
                    current.Branch = next.Label;
            }

            if (string.IsNullOrEmpty(listing.Last().Branch))
                throw new ArgumentException("Invalid listing: last instruction doesn't have a branch");

            return listing;
        }

        private static IList<MicroInstruction> FixAddresses(IList<MicroInstruction> listing, IDictionary<string, int> lookup)
        {
            foreach (var instruction in listing)
            {
                int address;

                if (lookup.TryGetValue(instruction.Label, out address))
                    instruction.Address = address;

                if (!lookup.TryGetValue(instruction.Branch, out address))
                    continue;

                instruction.Branch = ""; // we found the address
                instruction.OpCode.NextAddress = (ushort) address;
            }

            return listing.OrderBy(m => m.Address).ToList();
        }

        private static IList<MicroInstruction> FitInstructions(IList<MicroInstruction> listing, IDictionary<string, int> lookup)
        {
            var address = 0;
            var takenAddresses = new Dictionary<int, bool>();
            foreach (var value in lookup.Values)
                takenAddresses[value] = true;

            foreach (var instruction in listing)
            {
                if (lookup.ContainsKey(instruction.Label))
                    continue;

                while (takenAddresses.ContainsKey(address))
                    ++address;

                takenAddresses[address] = true;
                lookup[instruction.Label] = address;
                instruction.Address = address;
            }

            return listing;
        }
    }
}