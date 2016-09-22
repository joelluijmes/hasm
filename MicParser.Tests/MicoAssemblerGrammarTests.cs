using MicParser.Grammars;
using MicParser.OpCode;
using NUnit.Framework;
using ParserLib.Evaluation;

namespace MicParser.Tests
{
    [TestFixture]
    public sealed class MicoAssemblerGrammarTests
    {
        [Test]
        public void TestLeftInput()
        {
            var rule = MicroAssemblerGrammar.LeftInput;

            Assert.AreEqual(rule.ParseTree("H").FirstValue<long>(), (long)LeftRegister.H);
            Assert.AreEqual(rule.ParseTree("h").FirstValue<long>(), (long)LeftRegister.H);
            Assert.AreEqual(rule.ParseTree("1").FirstValue<long>(), (long)LeftRegister.One);
            Assert.AreEqual(rule.ParseTree("0").FirstValue<long>(), (long)LeftRegister.Zero);
            Assert.IsFalse(rule.Match("mdr"));
        }

        [Test]
        public void TestRightInput()
        {
            var rule = MicroAssemblerGrammar.RightInput;

            Assert.AreEqual(rule.ParseTree("CPP").FirstValue<long>(), (long)RightRegister.CPP);
            Assert.AreEqual(rule.ParseTree("mbr").FirstValue<long>(), (long)RightRegister.MBR);
            Assert.AreEqual(rule.ParseTree("tos").FirstValue<long>(), (long)RightRegister.TOS);
            Assert.IsFalse(rule.Match("h"));
        }

        [Test]
        public void TestDestination()
        {
            var rule = MicroAssemblerGrammar.Destination;

            Assert.AreEqual(rule.ParseTree("lv").FirstValue<long>(), (long)OutputRegister.LV);
            Assert.AreEqual(rule.ParseTree("mdr").FirstValue<long>(), (long)OutputRegister.MDR);
            Assert.AreEqual(rule.ParseTree("tos").FirstValue<long>(), (long)OutputRegister.TOS);
            Assert.IsFalse(rule.Match("1"));
        }

        [Test]
        public void TestOutput()
        {
            var rule = MicroAssemblerGrammar.Output;

            Assert.IsTrue(rule.Match("h="));
            Assert.IsTrue(rule.Match("h=sp="));
            Assert.IsFalse(rule.Match("h"));
        }


        [Test]
        public void TestAdd()
        {
            var rule = MicroAssemblerGrammar.Add;

            Assert.IsTrue(rule.Match("+"));
            Assert.IsTrue(rule.Match("Add"));
            Assert.AreEqual(rule.ParseTree("+").FirstValue<long>(), (long)ALU.Add);
        }

        [Test]
        public void TestSub()
        {
            var rule = MicroAssemblerGrammar.Sub;

            Assert.IsTrue(rule.Match("-"));
            Assert.IsTrue(rule.Match("sub"));
            Assert.AreEqual(rule.ParseTree("-").FirstValue<long>(), (long)ALU.Sub);
        }

        [Test]
        public void TestInvsereSub()
        {
            var rule = MicroAssemblerGrammar.InverseSub;

            Assert.IsTrue(rule.Match("-"));
            Assert.IsTrue(rule.Match("sub"));
            Assert.AreEqual(rule.ParseTree("-").FirstValue<long>(), (long)ALU.InverseSub);
        }

        [Test]
        public void TestAnd()
        {
            var rule = MicroAssemblerGrammar.LogicAnd;

            Assert.IsTrue(rule.Match("&"));
            Assert.IsTrue(rule.Match("and"));
            Assert.AreEqual(rule.ParseTree("&").FirstValue<long>(), (long)ALU.And);
        }

        [Test]
        public void TestOr()
        {
            var rule = MicroAssemblerGrammar.LogicOr;

            Assert.IsTrue(rule.Match("|"));
            Assert.IsTrue(rule.Match("or"));
            Assert.AreEqual(rule.ParseTree("|").FirstValue<long>(), (long)ALU.Or);
        }

        [Test]
        public void TestXor()
        {
            var rule = MicroAssemblerGrammar.LogicXor;

            Assert.IsTrue(rule.Match("^"));
            Assert.IsTrue(rule.Match("xor"));
            Assert.AreEqual(rule.ParseTree("^").FirstValue<long>(), (long)ALU.Xor);
        }

        [Test]
        public void TestClear()
        {
            var rule = MicroAssemblerGrammar.Clear;
            Assert.AreEqual(rule.ParseTree("clr").FirstValue<long>(), (long)ALU.Clear);
        }

        [Test]
        public void TestPreset()
        {
            var rule = MicroAssemblerGrammar.Preset;
            Assert.AreEqual(rule.ParseTree("Preset").FirstValue<long>(), (long)ALU.Preset);
        }

        [Test]
        public void TestTerm()
        {
            var rule = MicroAssemblerGrammar.Term;

            Assert.IsTrue(rule.Match("clr"));
            Assert.IsTrue(rule.Match($"{RightRegister.TOS}{ALU.Add}{LeftRegister.H}"));
            Assert.IsTrue(rule.Match($"{LeftRegister.H}{ALU.Add}{RightRegister.TOS}"));
            Assert.IsTrue(rule.Match($"{RightRegister.TOS}-{LeftRegister.H}"));
            Assert.IsTrue(rule.Match($"{LeftRegister.H}-{RightRegister.TOS}"));
            Assert.IsFalse(rule.Match($"{RightRegister.TOS}{ALU.Add}{RightRegister.TOS}"));
        }

        [Test]
        public void TestAlu()
        {
            var rule = MicroAssemblerGrammar.Alu;

            Assert.IsTrue(rule.Match("mar=sp+1;"));
            Assert.IsTrue(rule.Match("mar=sp+0;"));
            Assert.IsFalse(rule.Match("mar=sp+mar;"));
        }

        [Test]
        public void TestMemory()
        {
            var rule = MicroAssemblerGrammar.Memory;

            Assert.IsTrue(rule.Match("read;"));
            Assert.IsTrue(rule.Match("fetch;"));
            Assert.IsFalse(rule.Match("mar=sp+mar;"));
        }

        [Test]
        public void TestGoto()
        {
            var rule = MicroAssemblerGrammar.Branch;

            Assert.IsTrue(rule.Match("goto1;"));
            Assert.IsTrue(rule.Match("gotomain;"));
            Assert.IsFalse(rule.Match("mar=sp+mar;"));
        }
    }
}