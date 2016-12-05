using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using hasm.Parsing.Encoding;
using hasm.Parsing.Encoding.TypeConverters;

namespace hasm.Parsing.Models
{
    public sealed class Operation
    {
        private const int ENCODING_IMM = 10;
        private const int ENCODING_CONDITION = 16;
        private const int ENCODING_CONDITION_INVERTED = 19;
        private const int ENCODING_A = 20;
        private const int ENCODING_B = 24;
        private const int ENCODING_C = 28;
        private const int ENCODING_SP = 32;
        private const int ENCODING_ALU = 33;
        private const int ENCODING_CARRY = 36;
        private const int ENCODING_SHIFT = 37;
        private const int ENCODING_BREAK = 39;

        private static readonly Dictionary<string, AluOperation> _operations = new Dictionary<string, AluOperation>
        {
            ["-"] = AluOperation.Minus,
            ["+"] = AluOperation.Plus,
            ["&"] = AluOperation.And,
            ["|"] = AluOperation.Or,
            ["^"] = AluOperation.Xor
        };

        public static readonly Operation NOP = new Operation(null, null, null, AluOperation.Clear, false, false, RightShift.Disabled, Condition.None, false);

        [EncodableProperty(ENCODING_A, 4, Converter = typeof(LeftConverter))]
        private OperandConverter _leftOperand;

        [EncodableProperty(ENCODING_B, 4, Converter = typeof(RightConverter))]
        [EncodableProperty(ENCODING_IMM, 2, Converter = typeof(ImmediateConverter))]
        private OperandConverter _rightOperand;

        [EncodableProperty(ENCODING_C, 4, Converter = typeof(TargetConverter))]
        private OperandConverter _targetOperand;

        public Operation(string target, string left, string right, AluOperation aluOperation, bool carry, bool stackPointer, RightShift rightShift, Condition condition, bool inverted)
        {
            _targetOperand = new OperandConverter(target);
            _leftOperand = new OperandConverter(left);
            _rightOperand = new OperandConverter(right);

            Carry = carry;
            StackPointer = stackPointer;
            RightShift = rightShift;
            AluOperation = ((Left == null) && (Right != null)) || ((Left != null) && (Right == null))
                ? AluOperation.Plus // hardware uses addition with 0 to assign
                : aluOperation;

            FixOperands();

            Condition = condition;
            InvertedCondition = inverted;
        }

        private Operation(OperandConverter target, OperandConverter left, OperandConverter right, AluOperation aluOperation, bool carry, bool stackPointer, RightShift rightShift, Condition condition, bool inverted)
        {
            _targetOperand = target;
            _leftOperand = left;
            _rightOperand = right;
            Carry = carry;
            StackPointer = stackPointer;
            RightShift = rightShift;
            AluOperation = aluOperation;
            Condition = condition;
            InvertedCondition = inverted;
        }

        [EncodableProperty(ENCODING_CONDITION, 3)]
        public Condition Condition { get; set; } = Condition.None;

        [EncodableProperty(ENCODING_CONDITION_INVERTED)]
        public bool InvertedCondition { get; set; }

        [EncodableProperty(ENCODING_BREAK)]
        public bool Break { get; set; }

        [EncodableProperty(ENCODING_CARRY)]
        public bool Carry { get; set; }

        [EncodableProperty(ENCODING_SP)]
        public bool StackPointer { get; set; }

        [EncodableProperty(ENCODING_SHIFT, 2)]
        public RightShift RightShift { get; set; }

        [EncodableProperty(ENCODING_ALU, 3)]
        public AluOperation AluOperation { get; set; }

        public bool ExternalLeft { get; set; }
        public bool ExternalRight { get; set; }

        public string Target
        {
            get { return _targetOperand.Operand; }
            set
            {
                if (value.ToLower() == "sp")
                {
                    StackPointer = true;
                    _targetOperand.Operand = null;
                }
                else
                    _targetOperand.Operand = value;
            }
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

        private bool IsAssignment => ((AluOperation == AluOperation.Plus) && (Left == null) && (Right != null)) || ((Left != null) && (Right == null));

        public Operation Clone() => new Operation(_targetOperand, _leftOperand, _rightOperand, AluOperation, Carry, StackPointer, RightShift, Condition, InvertedCondition);

        public bool Equals(Operation other)
        {
            return string.Equals(Target, other.Target) &&
                   string.Equals(Left, other.Left) &&
                   string.Equals(Right, other.Right) &&
                   (Carry == other.Carry) &&
                   (StackPointer == other.StackPointer) &&
                   Equals(RightShift, other.RightShift) &&
                   (AluOperation == other.AluOperation) &&
                   (Condition == other.Condition) &&
                   (InvertedCondition == other.InvertedCondition);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as Operation;
            return (other != null) && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Target?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (int) Condition;
                hashCode = (hashCode*397) ^ InvertedCondition.GetHashCode();
                hashCode = (hashCode*397) ^ (Left?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Right?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ Carry.GetHashCode();
                hashCode = (hashCode*397) ^ StackPointer.GetHashCode();
                hashCode = (hashCode*397) ^ RightShift.GetHashCode();
                hashCode = (hashCode*397) ^ (int) AluOperation;
                return hashCode;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (Condition != Condition.None)
                builder.Append($"if {Condition} = {(InvertedCondition ? "0" : "1")}: ");

            if (!string.IsNullOrEmpty(Target))
                builder.Append($"{Target}=");
            if (StackPointer)
                builder.Append("SP=");

            if (AluOperation != AluOperation.InverseMinus)
            {
                if (!string.IsNullOrEmpty(Left))
                    builder.Append(Left);
                else
                {
                    if (!string.IsNullOrEmpty(Right))
                        builder.Append(Right);
                }

                if ((AluOperation != AluOperation.Clear) && !IsAssignment)
                {
                    var sign = _operations.FirstOrDefault(f => f.Value == AluOperation).Key;
                    if (!string.IsNullOrEmpty(sign))
                        builder.Append(sign);

                    builder.Append(Right);

                    if (Carry && !string.IsNullOrEmpty(sign))
                        builder.Append($"{sign}C");
                }
            }
            else
                builder.Append($"{Right}-{Left}");

            if (RightShift == RightShift.Logical)
                builder.Append(">>1");
            else
            {
                if (RightShift == RightShift.Arithmetic)
                    builder.Append(">>>1");
            }

            return builder.ToString();
        }

        private void FixOperands()
        {
            if (_leftOperand == OperandConverter.Invalid && _rightOperand == OperandConverter.Invalid)
                return;

            var swapped = false;
            if (_leftOperand.Bus == OperandInputBus.Right)
            {
                if (_rightOperand != OperandConverter.Invalid && !_rightOperand.Bus.HasFlag(OperandInputBus.Left))
                    throw new InvalidOperationException();

                var tmp = _rightOperand;
                _rightOperand = _leftOperand;
                _leftOperand = tmp;

                swapped = true;
            }
            else if (_rightOperand.Bus == OperandInputBus.Left)
            {
                if (_leftOperand != OperandConverter.Invalid && !_leftOperand.Bus.HasFlag(OperandInputBus.Right))
                    throw new InvalidOperationException();

                var tmp = _rightOperand;
                _rightOperand = _leftOperand;
                _leftOperand = tmp;

                swapped = true;
            }

            if (swapped && AluOperation == AluOperation.Minus)
                AluOperation = AluOperation.InverseMinus;
        }
    }
}
