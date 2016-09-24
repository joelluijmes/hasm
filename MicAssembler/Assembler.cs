using System;
using System.Collections.Generic;
using System.Linq;
using MicParser.Grammars;
using ParserLib.Evaluation;

namespace MicAssembler
{
    internal sealed class Assembler
    {
        private readonly List<string> _listing;

        public Assembler(IEnumerable<string> listing)
        {
            _listing = listing
                .Select(l => l.Trim()) // remove leading and trailing whitespace
                .Where(l => !string.IsNullOrEmpty(l)) // remove empty lines
                .ToList();
        }

        public void Parse()
        {
            var sections = ParseSections();

            List<string> content;
            if (sections.TryGetValue("init", out content))
                ParseInit(content);
            if (sections.TryGetValue("text", out content))
                ParseText(content);
        }

        private void ParseText(IList<string> content)
        {
        }

        private void ParseInit(IList<string> content)
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