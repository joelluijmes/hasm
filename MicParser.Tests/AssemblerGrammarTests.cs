using MicParser.Grammars;
using NUnit.Framework;
using ParserLib.Evaluation;

namespace MicParser.Tests
{
    [TestFixture]
    public sealed class AssemblerGrammarTests
    {
        [Test]
        public void TestSection()
        {
            var rule = AssemblerGrammar.Section;

            var tree = rule.ParseTree("section .text\r\nBIPUSH 5");

            Assert.AreEqual("text", tree.FirstValue<string>());
        }

        [Test]
        public void TestBIPUSH()
        {
            var rule = AssemblerGrammar.BIPUSH;

            var parsed = rule.ParseTree("bipush 0x20");
            Assert.AreEqual((int)Mnemonic.BIPUSH, parsed.FirstValueByName<int>("Mnemonic"));
            Assert.AreEqual(32, parsed.FirstValueByName<byte>("byte"));
        }

        [Test]
        public void TestDUP()
        {
            var rule = AssemblerGrammar.DUP;

            var parsed = rule.ParseTree("dup");
            Assert.AreEqual((int)Mnemonic.DUP, parsed.FirstValueByName<int>("Mnemonic"));
        }

        [Test]
        public void TestGOTO()
        {
            var rule = AssemblerGrammar.GOTO;

            var parsed = rule.ParseTree("goto 0x20");
            Assert.AreEqual((int)Mnemonic.GOTO, parsed.FirstValueByName<int>("Mnemonic"));
            Assert.AreEqual(32, parsed.FirstValueByName<short>("absolute"));

            parsed = rule.ParseTree("goto main");
            Assert.AreEqual("main", parsed.FirstValueByName<string>("label"));
        }

        [Test]
        public void TestIADD()
        {
            var rule = AssemblerGrammar.IADD;

            var parsed = rule.ParseTree("iadd");
            Assert.AreEqual((int)Mnemonic.IADD, parsed.FirstValueByName<int>("Mnemonic"));
        }

        [Test]
        public void TestIAND()
        {
            var rule = AssemblerGrammar.IAND;

            var parsed = rule.ParseTree("iand");
            Assert.AreEqual((int)Mnemonic.IAND, parsed.FirstValueByName<int>("Mnemonic"));
        }

        [Test]
        public void TestIFEQ()
        {
            var rule = AssemblerGrammar.IFEQ;

            var parsed = rule.ParseTree("IFEQ 0x20");
            Assert.AreEqual((int)Mnemonic.IFEQ, parsed.FirstValueByName<int>("Mnemonic"));
            Assert.AreEqual(32, parsed.FirstValueByName<short>("absolute"));

            parsed = rule.ParseTree("IFEQ main");
            Assert.AreEqual("main", parsed.FirstValueByName<string>("label"));
        }

        [Test]
        public void TestIFLT()
        {
            var rule = AssemblerGrammar.IFLT;

            var parsed = rule.ParseTree("IFLT 0x20");
            Assert.AreEqual((int)Mnemonic.IFLT, parsed.FirstValueByName<int>("Mnemonic"));
            Assert.AreEqual(32, parsed.FirstValueByName<short>("absolute"));

            parsed = rule.ParseTree("IFLT main");
            Assert.AreEqual("main", parsed.FirstValueByName<string>("label"));
        }

        [Test]
        public void TestIF_ICMPEQ()
        {
            var rule = AssemblerGrammar.IF_ICMPEQ;

            var parsed = rule.ParseTree("IF_ICMPEQ 0x20");
            Assert.AreEqual((int)Mnemonic.IF_ICMPEQ, parsed.FirstValueByName<int>("Mnemonic"));
            Assert.AreEqual(32, parsed.FirstValueByName<short>("absolute"));

            parsed = rule.ParseTree("IF_ICMPEQ main");
            Assert.AreEqual("main", parsed.FirstValueByName<string>("label"));
        }

        [Test]
        public void TestIINC()
        {
            var rule = AssemblerGrammar.IINC;

            var parsed = rule.ParseTree("IINC ab 0x20");
            Assert.AreEqual((int)Mnemonic.IINC, parsed.FirstValueByName<int>("Mnemonic"));
            Assert.AreEqual("ab", parsed.FirstValueByName<string>("var"));
            Assert.AreEqual(32, parsed.FirstValueByName<byte>("const"));

            parsed = rule.ParseTree("IINC 1 0x20");
            Assert.AreEqual(1, parsed.FirstValueByName<byte>("varnum"));
        }

        [Test]
        public void TestILOAD()
        {
            var rule = AssemblerGrammar.ILOAD;

            var parsed = rule.ParseTree("ILOAD ab");
            Assert.AreEqual((int)Mnemonic.ILOAD, parsed.FirstValueByName<int>("Mnemonic"));
            Assert.AreEqual("ab", parsed.FirstValueByName<string>("var"));

            parsed = rule.ParseTree("ILOAD 1");
            Assert.AreEqual(1, parsed.FirstValueByName<byte>("varnum"));
        }

        [Test]
        public void TestINVOKEVIRTUAL()
        {
            var rule = AssemblerGrammar.INVOKEVIRTUAL;

            var parsed = rule.ParseTree("INVOKEVIRTUAL 0x20");
            Assert.AreEqual((int)Mnemonic.INVOKEVIRTUAL, parsed.FirstValueByName<int>("Mnemonic"));
            Assert.AreEqual(32, parsed.FirstValueByName<short>("absolute"));

            parsed = rule.ParseTree("INVOKEVIRTUAL main");
            Assert.AreEqual("main", parsed.FirstValueByName<string>("label"));
        }

        [Test]
        public void TestIOR()
        {
            var rule = AssemblerGrammar.IOR;

            var parsed = rule.ParseTree("IOR");
            Assert.AreEqual((int)Mnemonic.IOR, parsed.FirstValueByName<int>("Mnemonic"));
        }

        [Test]
        public void TestIRETURN()
        {
            var rule = AssemblerGrammar.IRETURN;

            var parsed = rule.ParseTree("IRETURN");
            Assert.AreEqual((int)Mnemonic.IRETURN, parsed.FirstValueByName<int>("Mnemonic"));
        }

        [Test]
        public void TestISTORE()
        {
            var rule = AssemblerGrammar.ISTORE;

            var parsed = rule.ParseTree("ISTORE ab");
            Assert.AreEqual((int)Mnemonic.ISTORE, parsed.FirstValueByName<int>("Mnemonic"));
            Assert.AreEqual("ab", parsed.FirstValueByName<string>("var"));

            parsed = rule.ParseTree("ISTORE 1");
            Assert.AreEqual(1, parsed.FirstValueByName<byte>("varnum"));
        }

        [Test]
        public void TestISUB()
        {
            var rule = AssemblerGrammar.ISUB;

            var parsed = rule.ParseTree("ISUB");
            Assert.AreEqual((int)Mnemonic.ISUB, parsed.FirstValueByName<int>("Mnemonic"));
        }

        [Test]
        public void TestLDC_W()
        {
            var rule = AssemblerGrammar.LDC_W;

            var parsed = rule.ParseTree("LDC_W 0x20");
            Assert.AreEqual((int)Mnemonic.LDC_W, parsed.FirstValueByName<int>("Mnemonic"));
            Assert.AreEqual(32, parsed.FirstValueByName<short>("absolute"));

            parsed = rule.ParseTree("LDC_W main");
            Assert.AreEqual("main", parsed.FirstValueByName<string>("label"));
        }

        [Test]
        public void TestNOP()
        {
            var rule = AssemblerGrammar.NOP;

            var parsed = rule.ParseTree("NOP");
            Assert.AreEqual((int)Mnemonic.NOP, parsed.FirstValueByName<int>("Mnemonic"));
        }

        [Test]
        public void TestPOP()
        {
            var rule = AssemblerGrammar.POP;

            var parsed = rule.ParseTree("POP");
            Assert.AreEqual((int)Mnemonic.POP, parsed.FirstValueByName<int>("Mnemonic"));
        }

        [Test]
        public void TestSWAP()
        {
            var rule = AssemblerGrammar.SWAP;

            var parsed = rule.ParseTree("SWAP");
            Assert.AreEqual((int)Mnemonic.SWAP, parsed.FirstValueByName<int>("Mnemonic"));
        }

        [Test]
        public void TestWIDE()
        {
            var rule = AssemblerGrammar.WIDE;

            var parsed = rule.ParseTree("WIDE");
            Assert.AreEqual((int)Mnemonic.WIDE, parsed.FirstValueByName<int>("Mnemonic"));
        }
    }
}