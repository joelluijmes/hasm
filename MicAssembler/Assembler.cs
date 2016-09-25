using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MicParser;
using MicParser.Grammars;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace MicAssembler
{
    internal sealed class Assembler
    {
        private List<string> _listing;
        private Dictionary<string, string> _definitions;

        public Assembler(IEnumerable<string> listing)
        {
            _listing = listing
                .Select(l => l.Trim()) // remove leading and trailing whitespace
                .Where(l => !string.IsNullOrEmpty(l)) // remove empty lines
                .ToList();
        }

        public void Parse()
        {
            _definitions = ParseDefinitions();
            var sections = ParseSections();
            
            List<string> content;
            if (sections.TryGetValue("init", out content))
                ParseInit(content);
            if (sections.TryGetValue("text", out content))
                ParseText(content);
        }

        private Dictionary<string, string> ParseDefinitions()
        {
            var dic = new Dictionary<string, string>();
            var listing = new List<string>();
            foreach (var line in _listing)
            {
                if (!AssemblerGrammar.Definition.Match(line))
                {
                    listing.Add(line);
                    continue;
                }

                var tree = AssemblerGrammar.Definition.ParseTree(line);

                var key = tree.FirstValueByName<string>("key");
                var value = tree.FirstValueByName<string>("value");
                dic[key] = value;
            }

            _listing = listing;
            return dic;
        }

        private void ParseText(IList<string> content)
        {
            using (var f = File.CreateText("out.txt"))
            {
                foreach (var line in content)
                {
                    var tree = AssemblerGrammar.Instruction.ParseTree(line);
                    var assembled = ParseInstruction(tree);

                    f.WriteLine(assembled);
                }
            }
        }

        private string ParseInstruction(Node tree)
        {
            var mnemonic = tree.FirstValueByName<byte>(nameof(Mnemonic));
            if (tree.IsLeaf)
                return mnemonic.ToString("X2");

            var assembled = new List<byte>(new[] {mnemonic});
            foreach (var leaf in tree.Leafs.Skip(1))
            {
                if (leaf.Name == AssemblerGrammar.Byte.Name || leaf.Name == AssemblerGrammar.Const.Name || leaf.Name == AssemblerGrammar.Varnum.Name)
                {
                    assembled.Add(leaf.FirstValue<byte>());
                    continue;
                }

                var key = leaf.FirstValueNode<string>();
                if (key.Name != AssemblerGrammar.Var.Name)
                    throw new NotImplementedException();

                string value;
                if (!_definitions.TryGetValue(key.Value, out value))
                    throw new NotImplementedException();

                var rule = Grammar.MatchString("var", true) + Grammar.ConvertToValue("num", int.Parse, Grammar.Digits);
                var num = rule.ParseTree(value).FirstValueByName<int>("num");
                assembled.Add((byte) num);
            }

            return assembled.Select(a => a.ToString("X2")).Aggregate((a, b) => $"{a} {b}");
        }

        private void ParseInit(IEnumerable<string> content)
        {
            
        }

        private Dictionary<string, List<string>> ParseSections()
        {
            var sections = new Dictionary<string, List<string>>();

            // adds the section to the dictionary
            Action<Section> addSection = section =>
            {
                List<string> exisitingSection;
                if (sections.TryGetValue(section.Name, out exisitingSection))
                    exisitingSection.AddRange(section.Content); // merge it with previous sections (if found)
                else 
                    sections[section.Name] = section.Content; // set it
            };

            Section currentSection = null;
            foreach (var line in _listing)
            {
                if (AssemblerGrammar.Section.Match(line)) // new section
                {
                    var tree = AssemblerGrammar.Section.ParseTree(line);
                    var name = tree.FirstValueByName<string>(AssemblerGrammar.Section.Name);

                    if (currentSection != null) // add current one
                        addSection(currentSection);

                    currentSection = new Section(name.ToLower(), new List<string>());
                }
                else
                {
                    if (currentSection == null)
                        throw new InvalidOperationException();

                    currentSection.Content.Add(line);
                }
            }

            if (currentSection != null) // add the last section
                addSection(currentSection);

            return sections;
        }

        private class Section
        {
            public Section(string name, List<string> content)
            {
                Name = name;
                Content = content;
            }

            public string Name { get; }

            public List<string> Content { get; }
        }
    }
}