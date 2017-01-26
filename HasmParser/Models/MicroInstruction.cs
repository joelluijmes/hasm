using System.Text;
using hasm.Parsing.Encoding;
using hasm.Parsing.Encoding.TypeConverters;

namespace hasm.Parsing.Models
{
    public sealed class MicroInstruction
    {
        private const int ENCODING_NEXT = 0;
        private const int ENCODING_ADDR = 1;

        private const int ENCODING_STATUS_EN = 15;
        private const int ENCODING_MEMORY = 12;
        private const int ENCODING_BREAK = 39;

        public static readonly MicroInstruction NOP = new MicroInstruction(Operation.NOP, MemoryOperation.None, true, false, false) {InternalInstruction = true};

        private int _location;

        public MicroInstruction(Operation operation, MemoryOperation memory, bool lastInstruction, bool statusEnabled, bool breakEnabled)
        {
            Operation = operation;
            LastInstruction = lastInstruction;
            Memory = memory;
            StatusEnabled = statusEnabled;
            Break = breakEnabled;
        }

        public int Location
        {
            get
            {
                return InternalInstruction
                    ? _location | (1 << 15)
                    : _location;
            }
            set { _location = value; }
        }

        [EncodableProperty(ENCODING_NEXT)]
        public bool LastInstruction { get; } // NextMicroInstruction == null;

        [EncodableProperty(ENCODING_ADDR, 9)]
        public int NextInstruction => ((NextMicroInstruction?.Location + 1) & 0x7FFF) >> 6 ?? 0;

        [EncodableProperty(ENCODING_STATUS_EN)]
        public bool StatusEnabled { get; set; }

        [EncodableProperty(ENCODING_MEMORY, 3)]
        public MemoryOperation Memory { get; set; }
        
        [EncodableProperty(ENCODING_BREAK)]
        public bool Break { get; set; }

        [EncodableProperty(typeof(AluConverter), ExceedException = false)]
        public Operation Operation { get; set; }

        public MicroInstruction NextMicroInstruction { get; set; }

        public bool InternalInstruction { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append($"{Operation};");

            if (Memory != MemoryOperation.None)
                builder.Append($" {Memory};");

            if (LastInstruction)
                builder.Append(" next;");
            if (StatusEnabled)
                builder.Append(" status");

            return builder.ToString();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _location;
                hashCode = (hashCode*397) ^ LastInstruction.GetHashCode();
                hashCode = (hashCode*397) ^ StatusEnabled.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Memory;
                hashCode = (hashCode*397) ^ (Operation?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (NextMicroInstruction?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ InternalInstruction.GetHashCode();
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as MicroInstruction;
            return (other != null) && Equals(other);
        }

        public static bool operator ==(MicroInstruction left, MicroInstruction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MicroInstruction left, MicroInstruction right)
        {
            return !Equals(left, right);
        }

        public MicroInstruction Clone() => new MicroInstruction(Operation?.Clone(), Memory, LastInstruction, StatusEnabled, Break);

        private bool Equals(MicroInstruction other)
        {
            return (_location == other._location) &&
                   (LastInstruction == other.LastInstruction) &&
                   (StatusEnabled == other.StatusEnabled) &&
                   (Memory == other.Memory) &&
                   Operation.Equals(other.Operation) &&
                   (InternalInstruction == other.InternalInstruction) &&
                   Break == other.Break;
        }
    }
}
