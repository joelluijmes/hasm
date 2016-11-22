using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using hasm.Parsing.Encoding;
using hasm.Parsing.Encoding.TypeConverters;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace hasm.Parsing.Models
{
    public sealed class ALU
    {
        private const int ENCODING_IMM = 17;
        private const int ENCODING_A = 19;
        private const int ENCODING_B = 23;
        private const int ENCODING_C = 27;
        private const int ENCODING_SP = 31;
        private const int ENCODING_ALU = 32;
        private const int ENCODING_CARRY = 35;
        private const int ENCODING_SHIFT = 36;

        private static readonly Dictionary<string, AluOperation> _operations = new Dictionary<string, AluOperation>
        {
            ["-"] = AluOperation.Minus,
            ["+"] = AluOperation.Plus,
            ["&"] = AluOperation.And,
            ["|"] = AluOperation.Or,
            ["^"] = AluOperation.Xor
        };

        public static readonly ALU NOP = new ALU(null, null, null, AluOperation.Clear, false, false, false);

        [EncodableProperty(ENCODING_A, 4, Converter = typeof(LeftConverter))]
        private OperandConverter _leftOperand;

        [EncodableProperty(ENCODING_B, 4, Converter = typeof(RightConverter))]
        [EncodableProperty(ENCODING_IMM, 2, Converter = typeof(ImmediateConverter))]
        private OperandConverter _rightOperand;

        [EncodableProperty(ENCODING_C, 4, Converter = typeof(TargetConverter))]
        private OperandConverter _targetOperand;

        public ALU(string target, string left, string right, AluOperation operation, bool carry, bool stackPointer, bool rightShift)
        {
            _targetOperand = new OperandConverter(target);
            _leftOperand = new OperandConverter(left);
            _rightOperand = new OperandConverter(right);

            Carry = carry;
            StackPointer = stackPointer;
            RightShift = rightShift;
            Operation = ((Left == null) && (Right != null)) || ((Left != null) && (Right == null))
                ? AluOperation.Plus // hardware uses addition with 0 to assign
                : operation;

            FixOperands();
        }

        private ALU(OperandConverter target, OperandConverter left, OperandConverter right, AluOperation operation, bool carry, bool stackPointer, bool rightShift)
        {
            _targetOperand = target;
            _leftOperand = left;
            _rightOperand = right;
            Carry = carry;
            StackPointer = stackPointer;
            RightShift = rightShift;
            Operation = operation;
        }


        [EncodableProperty(ENCODING_CARRY)]
        public bool Carry { get; set; }

        [EncodableProperty(ENCODING_SP, Converter = typeof(InverseBooleanConverter))]
        public bool StackPointer { get; set; }

        [EncodableProperty(ENCODING_SHIFT)]
        public bool RightShift { get; set; }

        [EncodableProperty(ENCODING_ALU, 3)]
        public AluOperation Operation { get; set; }
        
        public bool ExternalLeft { get; set; }
        public bool ExternalRight { get; set; }

        public string Target
        {
            get { return _targetOperand.Operand; }
            set { _targetOperand.Operand = value; }
        }

        public string Left
        {
            get { return _leftOperand.Operand; }
            set
            {
                _leftOperand.Operand = value;
                FixImmediateOperand(ref _leftOperand);
            }
        }

        public string Right
        {
            get { return _rightOperand.Operand; }
            set
            {
                _rightOperand.Operand = value;
                FixImmediateOperand(ref _rightOperand);
            }
        }

        private bool IsAssignment => ((Operation == AluOperation.Plus) && (Left == null) && (Right != null)) || ((Left != null) && (Right == null));

        public ALU Clone() => new ALU(_targetOperand, _leftOperand, _rightOperand, Operation, Carry, StackPointer, RightShift);

        public bool Equals(ALU other)
        {
            return string.Equals(Target, other.Target) && string.Equals(Left, other.Left) && string.Equals(Right, other.Right) && (Carry == other.Carry) && (StackPointer == other.StackPointer) && Equals(RightShift, other.RightShift) && (Operation == other.Operation);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as ALU;
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

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(Target))
                builder.Append($"{Target}=");
            if (StackPointer)
                builder.Append("SP=");

            if (Operation != AluOperation.InverseMinus)
            {
                if (!string.IsNullOrEmpty(Left))
                    builder.Append(Left);
                else
                {
                    if (!string.IsNullOrEmpty(Right))
                        builder.Append(Right);
                }

                if ((Operation != AluOperation.Clear) && !IsAssignment)
                {
                    var sign = _operations.FirstOrDefault(f => f.Value == Operation).Key;
                    if (!string.IsNullOrEmpty(sign))
                        builder.Append(sign);

                    builder.Append(Right);

                    if (Carry && !string.IsNullOrEmpty(sign))
                        builder.Append($"{sign}C");
                }
            }
            else
                builder.Append($"{Right}-{Left}");

            if (RightShift)
                builder.Append(">>1");

            return builder.ToString();
        }

        public static ALU Parse(Node aluNode)
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

            return new ALU(target, left, right, operation, carry, stackPointer, shift);
        }

        private void FixOperands()
        {
            // we can't put negative on the bus, so we inverse the plus or minus and inverse the operand to fix this 
            FixImmediateOperand(ref _rightOperand);
            FixImmediateOperand(ref _leftOperand);

            // immediates must be placed on B bus (right), so for certain cases we have to swap the operands
            // so that the right is the immediate
            if (!_leftOperand.IsImmediate)
                return;

            if (_rightOperand == default(OperandConverter)) // assignment
            {
                _rightOperand = _leftOperand;
                _leftOperand = default(OperandConverter);
            }
            else
            {
                if (Operation == AluOperation.Minus) // com, neg (immedate - register)
                {
                    var temp = _rightOperand;
                    _rightOperand = _leftOperand;
                    _leftOperand = temp;

                    Operation = AluOperation.InverseMinus;
                }
                else
                    throw new NotImplementedException();
            }
        }

        private void FixImmediateOperand(ref OperandConverter operand)
        {
            int value;
            if (!operand.IsImmediate || !int.TryParse(operand.Operand, out value) || (value >= 0))
                return;

            operand.Operand = (value*-1).ToString();

            if (Operation == AluOperation.Minus)
                Operation = AluOperation.Plus;
            else
            {
                if (Operation == AluOperation.Plus)
                    Operation = AluOperation.Minus;
                else
                {
                    if (!IsAssignment)
                        throw new NotImplementedException();
                }
            }
        }
    }
}
