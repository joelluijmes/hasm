using MicParser.Grammars;
using NUnit.Framework;
using ParserLib.Evaluation;

namespace MicParser.Tests
{
    [TestFixture]
    public sealed class AssemblerGrammarTests
    {
        [Test]
        public void TestSectionName()
        {
            var rule = AssemblerGrammar.SectionName;

            Assert.AreEqual("text", rule.ParseTree("section .text").FirstValue<string>());
            Assert.IsFalse(rule.Match("iload"));
        }

        [Test]
        public void TestSectionContent()
        {
            var rule = AssemblerGrammar.SectionContent;

            Assert.AreEqual("BIPUSH 5\r\nISTORE multiplier\r\n", rule.ParseTree("BIPUSH 5\r\nISTORE multiplier\r\n").FirstValue<string>());
            Assert.AreEqual("BIPUSH 5\r\n", rule.ParseTree("BIPUSH 5\r\nsection .somewhere\r\nISTORE multiplier\r\n").FirstValue<string>());
        }

        [Test]
        public void TestSection()
        {
            var rule = AssemblerGrammar.Section;

            var tree = rule.ParseTree("section .text\r\nBIPUSH 5");

            Assert.AreEqual("text", tree.FirstValueByName<string>("Name"));
            Assert.AreEqual("BIPUSH 5", tree.FirstValueByName<string>("Content"));

        }
    }
}