using System.Collections.Generic;
using System.Linq;
using hasm.Parsing.Models;
using ParserLib.Evaluation;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm.Parsing.Grammars
{
    public sealed class MicroHasmGrammar : Grammar
    {
        private static readonly Dictionary<string, AluOperation> _operations = new Dictionary<string, AluOperation>
        {
            ["-"] = AluOperation.Minus,
            ["+"] = AluOperation.Plus,
            ["&"] = AluOperation.And,
            ["|"] = AluOperation.Or,
            ["^"] = AluOperation.Xor
        };

        private static readonly IDictionary<string, Condition> _conditions = new Dictionary<string, Condition>
        {
            ["C"] = Condition.Carry,
            ["V"] = Condition.Overflow,
            ["Z"] = Condition.Zero,
            ["S"] = Condition.Sign,
            ["N"] = Condition.Negative
        };

        private static readonly Rule _stackPointer = Optional(Text("SP", MatchString("SP", true)) + MatchChar('='));
        private static readonly Rule _targetRegister = Text("target", Label) + MatchChar('=');
        private static readonly Rule _target = (_targetRegister + _stackPointer) | (_stackPointer + _targetRegister);
        private static readonly Rule _left = Text("left", Label | Int32());
        private static readonly Rule _aluOperation = Text("op", MatchAnyString("+ - & | ^"));
        private static readonly Rule _right = Text("right", Label | Int32());
        private static readonly Rule _carry = Text("carry", PlusOrMinus + MatchChar('c', true));
        private static readonly Rule _if = Node("if", MatchString("if", true) + Text("status", Label) + MatchChar('=') + Text("cond", MatchChar('1') | MatchChar('0')) + MatchChar(':'));
        private static readonly Rule _nop = Text("nop", MatchString("nop", true) | End());
        private static readonly Rule _rightShift = (Text("lshift", MatchString(">>")) + MatchChar('1')) | (Text("ashift", MatchString(">>>")) + MatchChar('1')); // Left shift is implemented as DST + DST

        private static readonly Rule _assignment = _target + _left + Optional((_aluOperation + _right + Optional(_carry)) | _rightShift);
        private static readonly Rule _operation = (_left + _aluOperation + _right + Optional(_carry)) | _right;
        private static readonly Rule _aluRule = Optional(_if) + (_assignment | _operation);

        public static readonly Rule Alu = ConvertToValue("alu", ConvertToOperation, _aluRule);
        public static readonly Rule Operation = Node("operation", _nop | Alu);
        
        private static Operation ConvertToOperation(Node node)
        {
            var condition = Condition.None;
            var inverted = false;
            var ifNode = node.FirstNodeByNameOrDefault("if");
            if (ifNode != null)
            {
                var status = ifNode.FirstValueByNameOrDefault<string>("status");

                if (_conditions.TryGetValue(status, out condition))
                    inverted = ifNode.FirstValueByNameOrDefault<string>("cond") == "0";
            }

            var target = node.FirstValueByNameOrDefault<string>("target");
            var left = node.FirstValueByNameOrDefault<string>("left");
            var right = node.FirstValueByNameOrDefault<string>("right");

            var carry = node.FirstValueByNameOrDefault<string>("carry") != null;
            var stackPointer = node.FirstValueByNameOrDefault<string>("SP") != null;

            var rightShift = RightShift.Disabled;
            if (node.FirstValueByNameOrDefault<string>("ashift") != null)
                rightShift = RightShift.Arithmetic;
            else
            {
                if (node.FirstValueByNameOrDefault<string>("lshift") != null)
                    rightShift = RightShift.Logical;
            }

            var operation = AluOperation.Clear;
            var op = node.FirstValueByNameOrDefault<string>("op");
            if (op != null)
                _operations.TryGetValue(op, out operation);

            return new Operation(target, left, right, operation, carry, stackPointer, rightShift, condition, inverted);
        }
    }
}
