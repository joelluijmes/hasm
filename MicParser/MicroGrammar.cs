
using System;
using MicParser.InstructionTypes;
using MicParser.OpCode;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;
using RightRegister = MicParser.InstructionTypes.RightRegister;

namespace MicParser
{
    public sealed class MicroGrammar : Grammar
    {
        private static readonly Func<long, long, long> _accumulator = (a, b) => a | b;

        // ALU
        private static readonly Rule _leftInput       = ValueGrammar.FirstValue<long>("A",
            ValueGrammar.ConstantValue(LeftRegister.H.ToString(), (long) LeftRegister.H, MatchString("H", true)) |
            ValueGrammar.ConstantValue(LeftRegister.One.ToString(), (long) LeftRegister.One, MatchChar('1')) |
            ValueGrammar.ConstantValue(LeftRegister.Null.ToString(), (long) LeftRegister.Null, MatchChar('0')));
        private static readonly Rule _rightInput      = ValueGrammar.MatchEnum<RightRegister, long>("B");
        private static readonly Rule _destination     = ValueGrammar.MatchEnum<DestinationRegister, long>("C");
        private static readonly Rule _output          = ValueGrammar.AccumulateLeafs("Output", _accumulator, OneOrMore(_destination + MatchChar('=')));
        
        private static readonly Rule _add      = ValueGrammar.ConstantValue(AluOperation.Add.ToString(), (long)AluOperation.Add, SharedGrammar.MatchAnyString("add +", true));
        private static readonly Rule _sub = ValueGrammar.ConstantValue(AluOperation.Sub.ToString(), (long)AluOperation.Sub, SharedGrammar.MatchAnyString("sub -", true));
        private static readonly Rule _inverseSub = ValueGrammar.ConstantValue(AluOperation.InverseSub.ToString(), (long)AluOperation.InverseSub, SharedGrammar.MatchAnyString("sub -", true));
        private static readonly Rule _logicAnd = ValueGrammar.ConstantValue(AluOperation.And.ToString(), (long)AluOperation.And, SharedGrammar.MatchAnyString("and &", true));
        private static readonly Rule _logicOr  = ValueGrammar.ConstantValue(AluOperation.Or.ToString(), (long)AluOperation.Or, SharedGrammar.MatchAnyString("or |", true));
        private static readonly Rule _logicXor = ValueGrammar.ConstantValue(AluOperation.Xor.ToString(), (long)AluOperation.Xor, SharedGrammar.MatchAnyString("xor ^", true));
        private static readonly Rule _clear    = ValueGrammar.ConstantValue(AluOperation.Clear.ToString(), (long)AluOperation.Clear, MatchString("clr", true));
        private static readonly Rule _preset   = ValueGrammar.ConstantValue(AluOperation.Preset.ToString(), (long)AluOperation.Preset, MatchString("preset", true));
        private static readonly Rule _term     = ValueGrammar.AccumulateLeafs("Term", _accumulator,
            _clear |
            (_rightInput + _inverseSub + _leftInput) |
            (_leftInput + _sub + _rightInput) |
            Binary(_leftInput, _add | _logicAnd | _logicOr | _logicXor, _rightInput) |
            _preset);

        public static readonly Rule Alu = ValueGrammar.AccumulateLeafs("ALU", _accumulator, _output + _term) + MatchChar(';');

        // Memory
        public static readonly Rule Memory = ValueGrammar.MatchEnum<MemoryOperation, long>("Memory") + MatchChar(';');

        // Branching
        private static readonly Rule _label = ValueGrammar.Text("Label", (SharedGrammar.Letter | MatchChar('_')) + ZeroOrMore(SharedGrammar.Digit | SharedGrammar.Letter | MatchChar('_')));
        private static readonly Rule _nextInstruction = ValueGrammar.ConstantValue("Next", 1L << 9, MatchChar('(') + MatchString("MBR", true) + MatchChar(')'));
        private static readonly Rule _absolute = ValueGrammar.ConvertToValue("Absolute", long.Parse, SharedGrammar.Digits);

        public static readonly Rule Branch = MatchString("goto") + ValueGrammar.Text("Branch", _label | _nextInstruction | _absolute) + MatchChar(';');
        
        // Total :)
        private static readonly Rule _operation = ValueGrammar.AccumulateLeafs("Operation", _accumulator, Alu.Optional + Memory.Optional + Branch.Optional);
        public static readonly Rule Instruction = ValueGrammar.ConvertToValue("Instruction", MicroInstruction.FromNode, (_label + MatchChar(':')).Optional + _operation);
    }
}