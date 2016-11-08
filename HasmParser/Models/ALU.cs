using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using hasm.Parsing.Grammars;
using hasm.Parsing.Parsers;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace hasm.Parsing.Models
{
    public sealed class Alu
    {
        private static readonly Dictionary<string, AluOperation> _operations = new Dictionary<string, AluOperation>
        {
            ["-"] = AluOperation.Minus,
            ["+"] = AluOperation.Plus,
            ["&"] = AluOperation.And,
            ["|"] = AluOperation.Or,
            ["^"] = AluOperation.Xor
        };

        private OperandConverter _leftOperand;
        private OperandConverter _rightOperand;
        private OperandConverter _targetOperand;

        public string Target
        {
            get { return _targetOperand.Operand; }
            set { _targetOperand.Operand = value; }
        }

        public string Left
        {
            get { return _leftOperand.Operand; }
            set { _leftOperand.Operand = value; }
        }

        public string Right
        {
            get { return _rightOperand.Operand; }
            set { _rightOperand.Operand = value; }
        }

        public bool Carry { get; set; }
        public bool StackPointer { get; set; }
        public bool RightShift { get; set; }
        public AluOperation Operation { get; set; }

        public Alu(string target, string left, string right, AluOperation operation, bool carry, bool stackPointer, bool rightShift)
        {
            _leftOperand = new OperandConverter(left);
            _rightOperand = new OperandConverter(right);
            _targetOperand = new OperandConverter(target);

            // immediates must be placed on B bus (right), so for certain cases we have to swap the operands
            // so that the right is the immediate
            if (_leftOperand.IsImmediate)
            {
                if (_rightOperand == default(OperandConverter)) // assignment
                {
                    _rightOperand = _leftOperand;
                    _leftOperand = default(OperandConverter);
                }
                else if (operation == AluOperation.Minus) // com, neg (immedate - register)
                {
                    var temp = _rightOperand;
                    _rightOperand =  _leftOperand;
                    _leftOperand = temp;
                    
                    operation = AluOperation.InverseMinus;
                }
                else throw new NotImplementedException();
            }

            Carry = carry;
            StackPointer = stackPointer;
            RightShift = rightShift;
            Operation = operation;
        }

        public long Encode()
        {
            long result = 0;

            var disable = DisableLines.None;
            if (string.IsNullOrEmpty(Target))
                disable |= DisableLines.Target;
            else
                result |= _targetOperand.Convert() << 40;

            if (string.IsNullOrEmpty(Left))
                disable |= DisableLines.Left;
            else
                result |= _leftOperand.Convert() << 31;

            if (string.IsNullOrEmpty(Right))
                disable |= DisableLines.Right;
            else
            {
                if (_rightOperand.IsImmediate)
                {
                    result |= _rightOperand.Convert() << 18;
                    disable |= DisableLines.Right;
                }
                else
                {
                    result |= _rightOperand.Convert() << 36;
                    disable |= DisableLines.Immediate;
                }
            }

            if (StackPointer)
                result |= 1L << 45;
            if (Carry)
                result |= 1L << 55;
            if (RightShift)
                result |= 1L << 56;

            if (disable == DisableLines.None) // not everything can be enabled
                throw new NotImplementedException();

            result |= (long) disable << 46;

            return result;
        }

        public static Alu Parse(Node aluNode)
        {
            var target = aluNode.FirstValueByNameOrDefault<string>("target");
            var left = aluNode.FirstValueByNameOrDefault<string>("left");
            var right = aluNode.FirstValueByNameOrDefault<string>("right");

            var carry = aluNode.FirstValueByNameOrDefault<string>("carry") != null;
            var stackPointer = aluNode.FirstValueByNameOrDefault<string>("SP") != null;
            var shift = aluNode.FirstValueByNameOrDefault<string>("shift") != null;

            var operation = AluOperation.Clear;
            var op = aluNode.FirstValueByNameOrDefault<string>("op");
            if (op != null)
                _operations.TryGetValue(op, out operation);

            return new Alu(target, left, right, operation, carry, stackPointer, shift);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(Target))
                builder.Append($"{Target}=");
            if (StackPointer)
                builder.Append("SP=");
            if (!string.IsNullOrEmpty(Left))
                builder.Append(Left);

            if (Operation == AluOperation.InverseMinus)
            {
                builder.Append($"{Right}-{Left}");
            }
            else if (Operation != AluOperation.Clear)
            {
                var sign = _operations.FirstOrDefault(f => f.Value == Operation).Key;
                if (!string.IsNullOrEmpty(sign))
                    builder.Append(sign);

                builder.Append(Right);

                if (Carry && !string.IsNullOrEmpty(sign))
                    builder.Append($"{sign}C");
            }

            if (RightShift)
                builder.Append(">>1");

            return builder.ToString();
        }

        private bool Equals(Alu other)
        {
            return string.Equals(Target, other.Target) && string.Equals(Left, other.Left) && string.Equals(Right, other.Right) && (Carry == other.Carry) && (StackPointer == other.StackPointer) && string.Equals(RightShift, other.RightShift) && (Operation == other.Operation);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as Alu;
            return (other != null) && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Target?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (Left?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Right?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ Carry.GetHashCode();
                hashCode = (hashCode*397) ^ StackPointer.GetHashCode();
                hashCode = (hashCode*397) ^ RightShift.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Operation;
                return hashCode;
            }
        }

        public Alu Clone() => new Alu(Target, Left, Right, Operation, Carry, StackPointer, RightShift);

        private struct OperandConverter
        {
            private readonly OperandParser _parser;

            public string Operand { get; set; }
            public bool IsImmediate => this != default(OperandConverter) && ( _parser == null || _parser.OperandEncoding?.Type == OperandEncodingType.Range);

            public OperandConverter(string operand)
            { // operand can be null
                Operand = operand;
                _parser = HasmGrammar.FindOperandParser(operand);
            }

            public long Convert() => _parser?.Parse(Operand) ?? 0;

            public override string ToString() => $"{Operand}: {Convert()}";

            public bool Equals(OperandConverter other) => Equals(_parser, other._parser) && string.Equals(Operand, other.Operand);

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                return obj is OperandConverter && Equals((OperandConverter) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_parser?.GetHashCode() ?? 0)*397) ^ (Operand?.GetHashCode() ?? 0);
                }
            }

            public static bool operator ==(OperandConverter left, OperandConverter right) => left.Equals(right);

            public static bool operator !=(OperandConverter left, OperandConverter right) => !left.Equals(right);
        }
    }
}
