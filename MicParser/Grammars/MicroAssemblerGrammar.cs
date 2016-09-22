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
        public static readonly Rule LeftInput = FirstValue<long>("A",
            ConstantValue(LeftRegister.H.ToString(), (long)LeftRegister.H, MatchString("H", true)) |
            ConstantValue(LeftRegister.One.ToString(), (long)LeftRegister.One, MatchChar('1')) |
            ConstantValue(LeftRegister.Zero.ToString(), (long)LeftRegister.Zero, MatchChar('0')));

        public static readonly Rule RightInput = MatchEnum<RightRegister, long>("B");
        public static readonly Rule Destination = MatchEnum<OutputRegister, long>("C");
        public static readonly Rule Output = Accumulate("Output", _accumulator, OneOrMore(Destination + MatchChar('=')));

        public static readonly Rule Add = ConstantValue(ALU.Add.ToString(), (long) ALU.Add, MatchAnyString("add +", true));
        public static readonly Rule Sub = ConstantValue(ALU.Sub.ToString(), (long) ALU.Sub, MatchAnyString("sub -", true));
        public static readonly Rule InverseSub = ConstantValue(ALU.InverseSub.ToString(), (long) ALU.InverseSub, MatchAnyString("sub -", true));
        public static readonly Rule LogicAnd = ConstantValue(ALU.And.ToString(), (long) ALU.And, MatchAnyString("and &", true));
        public static readonly Rule LogicOr = ConstantValue(ALU.Or.ToString(), (long) ALU.Or, MatchAnyString("or |", true));
        public static readonly Rule LogicXor = ConstantValue(ALU.Xor.ToString(), (long) ALU.Xor, MatchAnyString("xor ^", true));
        public static readonly Rule Clear = ConstantValue(ALU.Clear.ToString(), (long) ALU.Clear, MatchString("clr", true));
        public static readonly Rule Preset = ConstantValue(ALU.Preset.ToString(), (long) ALU.Preset, MatchString("preset", true));

        public static readonly Rule Term = Accumulate("Term", _accumulator,
            Clear |
            (RightInput + InverseSub + LeftInput) |
            (LeftInput + Sub + RightInput) |
            Binary(LeftInput, Add | LogicAnd | LogicOr | LogicXor, RightInput) |
            Preset);

        public static readonly Rule Alu = Accumulate("ALU", _accumulator, Output + Term) + MatchChar(';');

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