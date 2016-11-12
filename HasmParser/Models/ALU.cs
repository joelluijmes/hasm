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

        private OperandConverter _leftOperand;
        private OperandConverter _rightOperand;
        private OperandConverter _targetOperand;

        private string _grammar;

        public ALU(string target, string left, string right, AluOperation operation, bool carry, bool stackPointer, bool rightShift)
        {
            _targetOperand = new OperandConverter(target);
            _leftOperand = new OperandConverter(left);
            _rightOperand = new OperandConverter(right);
            
            Carry = carry;
            StackPointer = stackPointer;
            RightShift = rightShift;
            Operation = (Left == null && Right != null) || (Left != null && Right == null)
                ? AluOperation.Plus   // hardware uses addition with 0 to assign
                : operation;

            FixOperands();
            _grammar = ToString();
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
            if (!operand.IsImmediate || !int.TryParse(operand.Operand, out value) || value >= 0)
                return;

            operand.Operand = (value*-1).ToString();

            if (Operation == AluOperation.Minus)
                Operation = AluOperation.Plus;
            else if (Operation == AluOperation.Plus)
                Operation = AluOperation.Minus;
            else if (!IsAssignment())
                throw new NotImplementedException();
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

        public bool Carry { get; set; }
        public bool StackPointer { get; set; }
        public bool RightShift { get; set; }
        public AluOperation Operation { get; set; }
        public bool ExternalImmediate { get; set; }

        public long Encode()
        {
            long result = 0;

            result |= (string.IsNullOrEmpty(Target)
                          ? 0xFL
                          : _targetOperand.Value) << ENCODING_C;

            result |= (string.IsNullOrEmpty(Left)
                          ? 0xFL
                          : _leftOperand.Value) << ENCODING_A;

            if (!string.IsNullOrEmpty(Right))
            {
                if (_rightOperand.IsImmediate)
                {
                    result |= (_rightOperand.Value & 0x3L) << ENCODING_IMM; // encode maximum of 12 bits in the encoding
                    result |= 0x9L << ENCODING_B; // internal immediate
                }
                else
                    result |= _rightOperand.Value << ENCODING_B;
            }
            else if (ExternalImmediate)
                result |= 0xAL << ENCODING_B;   // external immediate
            else
                result |= 0xFL << ENCODING_B;   // disable b 

            if (!StackPointer)  // stackpointter is disable in encoding
                result |= 1L << ENCODING_SP;
            if (Carry)
                result |= 1L << ENCODING_CARRY;
            if (RightShift)
                result |= 1L << ENCODING_SHIFT;

            result |= (long)Operation << ENCODING_ALU;

            return result;
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

        public void SetExternalImmediate()
        {
            if (_leftOperand.IsImmediate)
                _leftOperand = new OperandConverter();
            else if (_rightOperand.IsImmediate)
                _rightOperand = new OperandConverter();
            else
                throw new NotImplementedException();

            ExternalImmediate = true;
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
                else if (!string.IsNullOrEmpty(Right))
                    builder.Append(Right);

                if (Operation != AluOperation.Clear && !IsAssignment())
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

        public ALU Clone() => new ALU(_targetOperand, _leftOperand, _rightOperand, Operation, Carry, StackPointer, RightShift) { _grammar = _grammar };

        private bool IsAssignment() => Operation == AluOperation.Plus && (Left == null && Right != null) || (Left != null && Right == null);

        private struct OperandConverter
        {
            private readonly OperandParser _parser;
            private string _operand;

            public string Operand
            {
                get { return _operand; }
                set
                {
                    _operand = value;

                    if (_operand != value) // if changes -> reset the converted value
                        this._value = null;
                }
            }

            public bool IsImmediate => (this != default(OperandConverter)) && ((_parser == null) || (_parser.OperandEncoding?.Type == OperandEncodingType.Range));

            public OperandConverter(string operand)
            { // operand can be null
                _operand = operand;
                _parser = HasmGrammar.FindOperandParser(operand);
                _value = null;
            }

            public long? _value;
            public long Value => _value ?? (_value = _parser?.Parse(Operand)) ?? 0;

            public override string ToString() => $"{Operand}: {Value}";

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
