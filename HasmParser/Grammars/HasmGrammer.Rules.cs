using System;
using System.Collections.Generic;
using ParserLib;
using ParserLib.Evaluation;
using ParserLib.Evaluation.Rules;
using ParserLib.Exceptions;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Grammars
{
    public sealed partial class HasmGrammar
    {
        private static readonly Rule _anythingTillComment = Text("text", MatchWhile(Not(MatchChar(';')) + MatchAnyChar()));

        /// <summary>
        ///     Rule for opcode.
        /// </summary>
        public static readonly ValueRule<string> Opcode = Text("opcode", Label);

        /// <summary>
        ///     Rule for label defined as label + ':'
        /// </summary>
        public static readonly ValueRule<string> AssemblyLabel = FirstValue<string>("label", Text(Label) + MatchChar(':'));

        public static readonly ValueRule<string> AssemblyDirective = FirstValue<string>("directive", MatchChar('.') + Text(Label));

        /// <summary>
        ///     The listing instruction, just about anything till the end or ';'
        /// </summary>
        public static readonly ValueRule<string> AssemblyInstruction = FirstValue<string>("instruction", _anythingTillComment);

        public static readonly ValueRule<string> Operands = FirstValue<string>("operands", _anythingTillComment);

        /// <summary>
        ///     The listing comment, must start with ';', matches till the end.
        /// </summary>
        public static readonly ValueRule<string> AssemblyComment = FirstValue<string>("comment", MatchChar(';') + Text(MatchWhile(MatchAnyChar())));

        public static readonly Rule Line = (Optional(AssemblyLabel) + Optional(Whitespace) + (AssemblyDirective | AssemblyInstruction) + Optional(Whitespace) + Optional(Operands) + Optional(Whitespace) + Optional(AssemblyComment)) | AssemblyComment | End();

        public static readonly Rule DirectiveEqual = Text("label", Label) + MatchChar('=') + Int32("value");

        public static readonly Rule DirectiveDefine = Text("label", Label) + MatchChar('=') + _anythingTillComment;

        public static readonly Rule DefineByte = Int8() + Optional(OneOrMore(MatchChar(',') + Optional(Whitespace) + Int8()));

        private static readonly Rule _expressionA;
        private static readonly Rule _expressionFunc = Func(() => _expressionA);

        public static readonly Rule Expression = Int32("left") + _expressionFunc;
        
    //(Text("left", Label) | Int32("left")) + Optional(Whitespace.Optional + Text("operation", MatchAnyString("+ - << >> * / %")) + Whitespace.Optional + Int32("right"));

        public static int Evaluate(Node tree, IDictionary<string, string> lookup)
        {
            var left = tree.FirstNodeByName("left");
            var leftValue = 0;
            if (left.IsValueNode<string>())
            {
                var leftStringValue = left.FirstValue<string>();

                string lookupValue;
                if (!lookup.TryGetValue(leftStringValue, out lookupValue))
                { }

                if (!int.TryParse(lookupValue, out leftValue))
                    throw new NotImplementedException();
            }
            else if (left.IsValueNode<int>())
                leftValue = left.FirstValue<int>();

            var operation = tree.FirstValueByNameOrDefault<string>("operation");
            if (operation == null)
                return leftValue;

            var rightValue = tree.FirstValueByName<int>("right");

            switch (operation)
            {
            case "+":
                return leftValue + rightValue;
            case "-":
                return leftValue - rightValue;
            case "*":
                return leftValue*rightValue;
            case "/":
                return leftValue/rightValue;
            case "%":
                return leftValue%rightValue;
            case "<<":
                return leftValue << rightValue;
            case ">>":
                return leftValue >> rightValue;
            case "|":
                return leftValue | rightValue;
            case "&":
                return leftValue & rightValue;
            default:
                throw new ParserException($"Couldn't parse tree. Unkown operation '{operation}'\r\nMatched tree: {tree.PrettyFormat()}");
            }
        }
    }
}
