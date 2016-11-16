using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using hasm.Parsing.Encoding;
using hasm.Parsing.Encoding.TypeConverters;
using hasm.Parsing.Grammars;
using OfficeOpenXml;
using ParserLib.Evaluation;
using ParserLib.Parsing;

namespace hasm.Parsing.Models
{
    public sealed class MicroInstruction
    {
        

        private const int ENCODING_NEXT = 0;
        private const int ENCODING_ADDR = 1;
        private const int ENCODING_CONDITION = 10;
        private const int ENCODING_CONDITION_INVERTED = 13;
        private const int ENCODING_STATUS_EN = 14;
        private const int ENCODING_MEMORY = 15;

        private static readonly Dictionary<string, Condition> _conditions = new Dictionary<string, Condition>
        {
            ["C"] = Condition.Carry,
            ["V"] = Condition.Overflow,
            ["Z"] = Condition.Zero,
            ["S"] = Condition.Sign,
            ["N"] = Condition.Negative
        };

        private int _location;

        public static readonly MicroInstruction NOP = new MicroInstruction(ALU.NOP, MemoryOperation.None, true, false, Condition.None, false) {InternalInstruction = true};

        public int Location
        {
            get
            {
                return InternalInstruction
                    ? _location | 1 << 15
                    : _location;
            }
            set { _location = value; }
        }

        [EncodableProperty(ENCODING_NEXT)]
        public bool LastInstruction { get;} // NextMicroInstruction == null;

        [EncodableProperty(ENCODING_ADDR, 9)]
        public int NextInstruction => 
            (NextMicroInstruction?.Location & 0x7FFF) >> 6 ?? 0;

        [EncodableProperty(ENCODING_CONDITION, 3)]
        public Condition Condition { get; set; }

        [EncodableProperty(ENCODING_CONDITION_INVERTED)]
        public bool InvertedCondition { get; set; }

        [EncodableProperty(ENCODING_STATUS_EN)]
        public bool StatusEnabled { get; set; }

        [EncodableProperty(ENCODING_MEMORY, 2)]
        public MemoryOperation Memory { get; set; }

        [EncodableProperty(typeof(AluConverter), ExceedException = false)]
        public ALU ALU { get; set; }

        public MicroInstruction NextMicroInstruction { get; set; }

        public bool InternalInstruction { get; set; }

        public MicroInstruction(ALU alu, MemoryOperation memory, bool lastInstruction, bool statusEnabled, Condition condition, bool invertedCondition)
        {
            ALU = alu;
            LastInstruction = lastInstruction;
            Memory = memory;
            StatusEnabled = statusEnabled;
            Condition = condition;
            InvertedCondition = invertedCondition;
        }
        
        public static MicroInstruction Parse(string[] row)
        {
            var operation = new Regex("\\s+").Replace(row[1], "");
            var parsed = MicroHasmGrammar.Operation.ParseTree(operation);

            var condition = Condition.None;
            var inverted = false;
            if (parsed.FirstNodeByNameOrDefault("if") != null)
            {
                var status = parsed.FirstValueByNameOrDefault<string>("status");
 
                if (_conditions.TryGetValue(status, out condition))
                     inverted = parsed.FirstValueByNameOrDefault<string>("cond") == "0";
            }
            
            var aluNode = parsed.FirstNodeByNameOrDefault("alu");
            var alu = aluNode != null
                ? ALU.Parse(aluNode)
                : ALU.NOP;

            var memoryCell = row[2];
            var memory = string.IsNullOrEmpty(memoryCell)
                ? MemoryOperation.None 
                : Grammar.EnumValue<MemoryOperation>().FirstValueOrDefault(memoryCell);

            var gotoInstruction = row[3];
            var lastInstruction = gotoInstruction == "next";

            var statusCell = row[4];
            var statusEnabled = statusCell == "1";
            
            return new MicroInstruction(alu, memory, lastInstruction, statusEnabled, condition, inverted);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (Condition != Condition.None)
                builder.Append($"if {Condition} = {(InvertedCondition ? "0" : "1")}: ");

            builder.Append($"{ALU};");

            if (Memory != MemoryOperation.None)
                builder.Append($" {Memory};");

            if (LastInstruction)
                builder.Append(" next;");
            if (StatusEnabled)
                builder.Append(" status");

            return builder.ToString();
        }

        private bool Equals(MicroInstruction other)
        {
            return _location == other._location && LastInstruction == other.LastInstruction && Condition == other.Condition && InvertedCondition == other.InvertedCondition && StatusEnabled == other.StatusEnabled && Memory == other.Memory && Equals(ALU, other.ALU) && Equals(NextMicroInstruction, other.NextMicroInstruction) && InternalInstruction == other.InternalInstruction;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _location;
                hashCode = (hashCode*397) ^ LastInstruction.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Condition;
                hashCode = (hashCode*397) ^ InvertedCondition.GetHashCode();
                hashCode = (hashCode*397) ^ StatusEnabled.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Memory;
                hashCode = (hashCode*397) ^ (ALU != null
                               ? ALU.GetHashCode()
                               : 0);
                hashCode = (hashCode*397) ^ (NextMicroInstruction != null
                               ? NextMicroInstruction.GetHashCode()
                               : 0);
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
            return other != null && Equals(other);
        }

        public static bool operator ==(MicroInstruction left, MicroInstruction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MicroInstruction left, MicroInstruction right)
        {
            return !Equals(left, right);
        }

        public MicroInstruction Clone() => new MicroInstruction(ALU?.Clone(), Memory, LastInstruction, StatusEnabled, Condition, InvertedCondition);
    }
}