using hasm.Parsing.Grammars;
using hasm.Parsing.Parsers;

namespace hasm.Parsing.Models
{
    public sealed partial class ALU
    {
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
                        _value = null;
                }
            }

            public bool IsImmediate => (this != default(OperandConverter)) && ((_parser == null) || (_parser.OperandEncoding?.Type == OperandEncodingType.Range));

            public OperandConverter(string operand)
            { // operand can be null
                _operand = operand;
                _parser = HasmGrammar.FindOperandParser(operand);
                _value = null;
            }

            private long? _value;
            public long Value => _value ?? (_value = _parser?.Parse(Operand)) ?? 0;

            public override string ToString() => $"{Operand}: {Value}";

            private bool Equals(OperandConverter other) => Equals(_parser, other._parser) && string.Equals(Operand, other.Operand);

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
