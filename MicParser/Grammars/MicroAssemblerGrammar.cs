using System;
using MicParser.OpCode;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace MicParser.Grammars
{
    public sealed class MicroAssemblerGrammar : Grammar
    {
        private static readonly Func<long, long, long> _accumulator = (a, b) => a | b;

        // ALU
        private static readonly Rule _leftInput = FirstValue<long>("A",
            ConstantValue(ALU.H.ToString(), (long) ALU.H, MatchString("H", true)) |
            ConstantValue(ALU.One.ToString(), (long) ALU.One, MatchChar('1')) |
            ConstantValue(ALU.Zero.ToString(), (long) ALU.Zero, MatchChar('0')));

        private static readonly Rule _rightInput = MatchEnum<RightRegister, long>("B");
        private static readonly Rule _destination = MatchEnum<OutputRegister, long>("C");
        private static readonly Rule _output = Accumulate("Output", _accumulator, OneOrMore(_destination + MatchChar('=')));

        private static readonly Rule _add = ConstantValue(ALU.Add.ToString(), (long) ALU.Add, MatchAnyString("add +", true));
        private static readonly Rule _sub = ConstantValue(ALU.Sub.ToString(), (long) ALU.Sub, MatchAnyString("sub -", true));
        private static readonly Rule _inverseSub = ConstantValue(ALU.InverseSub.ToString(), (long) ALU.InverseSub, MatchAnyString("sub -", true));
        private static readonly Rule _logicAnd = ConstantValue(ALU.And.ToString(), (long) ALU.And, MatchAnyString("and &", true));
        private static readonly Rule _logicOr = ConstantValue(ALU.Or.ToString(), (long) ALU.Or, MatchAnyString("or |", true));
        private static readonly Rule _logicXor = ConstantValue(ALU.Xor.ToString(), (long) ALU.Xor, MatchAnyString("xor ^", true));
        private static readonly Rule _clear = ConstantValue(ALU.Clear.ToString(), (long) ALU.Clear, MatchString("clr", true));
        private static readonly Rule _preset = ConstantValue(ALU.Preset.ToString(), (long) ALU.Preset, MatchString("preset", true));

        private static readonly Rule _term = Accumulate("Term", _accumulator,
            _clear |
            (_rightInput + _inverseSub + _leftInput) |
            (_leftInput + _sub + _rightInput) |
            Binary(_leftInput, _add | _logicAnd | _logicOr | _logicXor, _rightInput) |
            _preset);

        public static readonly Rule Alu = Accumulate("ALU", _accumulator, _output + _term) + MatchChar(';');

        // Memory
        public static readonly Rule Memory = MatchEnum<Memory, long>("Memory") + MatchChar(';');

        // Branching
        private static readonly Rule _nextInstruction = ConstantValue("Next", 1L << 9, MatchChar('(') + MatchString("MBR", true) + MatchChar(')'));
        private static readonly Rule _absolute = ConvertToValue("Absolute", long.Parse, Digits);

        public static readonly Rule Branch = MatchString("goto") + Text("Branch", Label | _nextInstruction | _absolute) + MatchChar(';');

        // Total :)
        private static readonly Rule _operation = Accumulate("Operation", _accumulator, Alu.Optional + Memory.Optional + Branch.Optional);
        public static readonly Rule Instruction = ConvertToValue("Instruction", MicroInstruction.FromNode, (Label + MatchChar(':')).Optional + _operation);
    }
}