using System;
using MicParser.OpCode;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace MicParser
{
    public sealed class MicroGrammar : Grammar
    {
        private static readonly Func<long, long, long> _accumulator = (a, b) => a | b;

        // ALU
        private static readonly Rule _leftInput = ValueGrammar.FirstValue<long>("A",
            ValueGrammar.ConstantValue(ALU.H.ToString(), (long) ALU.H, MatchString("H", true)) |
            ValueGrammar.ConstantValue(ALU.One.ToString(), (long) ALU.One, MatchChar('1')) |
            ValueGrammar.ConstantValue(ALU.Zero.ToString(), (long) ALU.Zero, MatchChar('0')));

        private static readonly Rule _rightInput = ValueGrammar.MatchEnum<RightRegister, long>("B");
        private static readonly Rule _destination = ValueGrammar.MatchEnum<OutputRegister, long>("C");
        private static readonly Rule _output = ValueGrammar.Accumulate("Output", _accumulator, OneOrMore(_destination + MatchChar('=')));

        private static readonly Rule _add = ValueGrammar.ConstantValue(ALU.Add.ToString(), (long) ALU.Add, MatchAnyString("add +", true));
        private static readonly Rule _sub = ValueGrammar.ConstantValue(ALU.Sub.ToString(), (long) ALU.Sub, MatchAnyString("sub -", true));
        private static readonly Rule _inverseSub = ValueGrammar.ConstantValue(ALU.InverseSub.ToString(), (long) ALU.InverseSub, MatchAnyString("sub -", true));
        private static readonly Rule _logicAnd = ValueGrammar.ConstantValue(ALU.And.ToString(), (long) ALU.And, MatchAnyString("and &", true));
        private static readonly Rule _logicOr = ValueGrammar.ConstantValue(ALU.Or.ToString(), (long) ALU.Or, MatchAnyString("or |", true));
        private static readonly Rule _logicXor = ValueGrammar.ConstantValue(ALU.Xor.ToString(), (long) ALU.Xor, MatchAnyString("xor ^", true));
        private static readonly Rule _clear = ValueGrammar.ConstantValue(ALU.Clear.ToString(), (long) ALU.Clear, MatchString("clr", true));
        private static readonly Rule _preset = ValueGrammar.ConstantValue(ALU.Preset.ToString(), (long) ALU.Preset, MatchString("preset", true));

        private static readonly Rule _term = ValueGrammar.Accumulate("Term", _accumulator,
            _clear |
            (_rightInput + _inverseSub + _leftInput) |
            (_leftInput + _sub + _rightInput) |
            Binary(_leftInput, _add | _logicAnd | _logicOr | _logicXor, _rightInput) |
            _preset);

        public static readonly Rule Alu = ValueGrammar.Accumulate("ALU", _accumulator, _output + _term) + MatchChar(';');

        // Memory
        public static readonly Rule Memory = ValueGrammar.MatchEnum<Memory, long>("Memory") + MatchChar(';');

        // Branching
        public static readonly Rule Label = ValueGrammar.Text("Label", (SharedGrammar.Letter | MatchChar('_')) + ZeroOrMore(SharedGrammar.Digit | SharedGrammar.Letter | MatchChar('_')));
        private static readonly Rule _nextInstruction = ValueGrammar.ConstantValue("Next", 1L << 9, MatchChar('(') + MatchString("MBR", true) + MatchChar(')'));
        private static readonly Rule _absolute = ValueGrammar.ConvertToValue("Absolute", long.Parse, SharedGrammar.Digits);

        public static readonly Rule Branch = MatchString("goto") + ValueGrammar.Text("Branch", Label | _nextInstruction | _absolute) + MatchChar(';');

        // Total :)
        private static readonly Rule _operation = ValueGrammar.Accumulate("Operation", _accumulator, Alu.Optional + Memory.Optional + Branch.Optional);
        public static readonly Rule Instruction = ValueGrammar.ConvertToValue("Instruction", MicroInstruction.FromNode, (Label + MatchChar(':')).Optional + _operation);
    }
}