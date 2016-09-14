using System;
using System.Collections.Generic;
using System.Linq;
using MicParser.NodeTypes;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;
using ParserLib.Parsing.Value;

namespace MicParser
{
    public sealed class MicroGrammar : Grammar
    {
        public static readonly Rule _label = Value<string>("Label", n => n.Value, SharedGrammar.Letter | MatchChar('_')) + ZeroOrMore(SharedGrammar.Digit | SharedGrammar.Letter | MatchChar('_'));
        private static readonly Rule _gotoMBR = Value("next", 1L << 9, MatchChar('(') + MatchString("MBR", true) + MatchChar(')'));
        private static readonly Rule _constant = Value("constant", long.Parse, SharedGrammar.Digits);

        //private static readonly Rule _h = Node(NodeType.H, MatchString("H", true));
        //private static readonly Rule _constant = Node(NodeType.Constant, MatchChar('1') | MatchChar('0'));
        private static readonly Rule _left = Value<long>(NodeType.A, GetFirstValueFromLeafs,
            Value(LeftRegister.H.ToString(), (long) LeftRegister.H, MatchString("H", true)) |
            Value(LeftRegister.One.ToString(), (long) LeftRegister.One, MatchChar('1')) |
            Value(LeftRegister.Null.ToString(), (long) LeftRegister.Null, MatchChar('0')));

        private static readonly Rule _right = Value<long>("B", GetFirstValueFromLeafs, Or(GetValueRulesFromEnum<RightRegister, long>()));
        private static readonly Rule _destination = Value<long>("Destination", GetFirstValueFromLeafs, Or(GetValueRulesFromEnum<DestinationRegister, long>()));

        private static readonly Rule _add = Value(AluOperation.Add, (long) AluOperation.Add, SharedGrammar.MatchAnyString("add +", true));
        private static readonly Rule _sub = Value(AluOperation.Sub, (long) AluOperation.Sub, SharedGrammar.MatchAnyString("sub -", true));
        private static readonly Rule _logicAnd = Value(AluOperation.And, (long) AluOperation.And, SharedGrammar.MatchAnyString("and &", true));
        private static readonly Rule _logicOr = Value(AluOperation.Or, (long) AluOperation.Or, SharedGrammar.MatchAnyString("or |", true));
        private static readonly Rule _logicXor = Value(AluOperation.Xor, (long) AluOperation.Xor, SharedGrammar.MatchAnyString("xor ^", true));
        private static readonly Rule _clear = Value(AluOperation.Clear, (long) AluOperation.Clear, MatchString("clr", true));
        private static readonly Rule _preset = Value(AluOperation.Preset, (long) AluOperation.Preset, MatchString("preset", true));

        private static readonly Rule _termRule = _clear 
            | (_right + _sub + _left) 
            | (_left + _sub + _right) 
            | Binary(_left, _add | _logicAnd | _logicOr | _logicXor, _right) 
            | _preset;
        private static readonly Rule _term = Value<long>(NodeType.Term, GetValue, _termRule);

        public static readonly Rule Alu = Value<long>(NodeType.ALU, GetValue, OneOrMore(_destination + MatchChar('=')) + _term);
        public static readonly Rule Memory = Value<long>(NodeType.Memory, GetFirstValueFromLeafs, Or(GetValueRulesFromEnum<MemoryOperation, long>()));
        public static readonly Rule Goto = Value<long>(NodeType.Branch, GetFirstValueFromLeafs, MatchString("goto") + Node("operand", _gotoMBR | _constant));

        private static readonly Rule _cycle = Value<long>(NodeType.Statement, GetValue, (Alu + MatchChar(';')).Optional + (Memory + MatchChar(';')).Optional + (Goto + MatchChar(';')).Optional + End());
        public static readonly Rule Statement = Value<MicroInstruction>(NodeType.Instruction, GetFirstValueFromLeafs, _label + MatchChar(':') + _cycle);

        private static Rule Node(NodeType type, Rule rule) => Node(type.ToString(), rule);
        private static Rule Node(AluOperation operation, Rule rule) => Node(operation.ToString(), rule);

        private static Rule Value<T>(NodeType type, Func<ValueNode<T>, T> valueFunc, Rule rule) => Value(type.ToString(), valueFunc, rule);
        private static Rule Value<T>(AluOperation operation, T value, Rule rule) => Value(operation.ToString(), value, rule);

        private static long GetValue(Node root) => GetValue<long>(root, (a, b) => a | b);

        private static TType GetValue<TType>(Node root, Func<TType, TType, TType> operation)
        {
            var value = default(TType);
            foreach (var leaf in root.Leafs)
                value = operation(value, GetValueFromLeafs(leaf, operation, value));

            return value;
        }

        private static TType GetValueFromLeafs<TType>(Node root, Func<TType, TType, TType> operation, TType current)
        {
            TType value;
            if (GetValueOrDefault(root, out value))
                return operation(current, value);

            foreach (var leaf in root.Leafs)
                current = operation(current, GetValueFromLeafs(leaf, operation, current));

            return default(TType);
        }

        private static bool GetValueOrDefault<TType>(Node node, out TType value)
        {
            var valueNode = node as ValueNode<TType>;
            var isValueNode = valueNode != null;

            value = isValueNode ? valueNode.Value : default(TType);
            return isValueNode;
        }

        private static TType GetFirstValueFromLeafs<TType>(ValueNode<TType> root)
        {
            var node = FindLeaf(root, n => n is ValueNode<TType> && (n != root));
            if (node == null)
                throw new NotImplementedException();

            return ((ValueNode<TType>) node).Value;
        }

        private static Node FindLeaf(Node root, Predicate<Node> predicate)
        {
            if (predicate(root))
                return root;

            foreach (var leaf in root.Leafs)
            {
                var subLeaf = FindLeaf(leaf, predicate);
                if (subLeaf != null)
                    return subLeaf;
            }

            return null;
        }

        private static IEnumerable<Rule> GetValueRulesFromEnum<TEnum, TType>()
        {
            var type = typeof(TEnum);
            if (!type.IsEnum)
                throw new ArgumentException("TEnum must be an enum");

            var keyValues = MapEnumToKeyValue<TEnum, TType>();
            return keyValues.Select(k => Value(k.Key, k.Value, MatchString(k.Key, true)));
        }

        private static IEnumerable<KeyValuePair<string, TType>> MapEnumToKeyValue<TEnum, TType>()
        {
            var names = Enum.GetNames(typeof(TEnum));
            Func<string, TType> getValueFunc = s =>
                    (TType) Enum.Parse(typeof(TEnum), s);

            return names.Select(n => new KeyValuePair<string, TType>(n, getValueFunc(n)));
        }
    }
}