using System;

namespace hasm.Parsing.Encoding
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EncodablePropertyAttribute : Attribute
    {
        public int Start { get; }
        public int Count { get; }
        public bool ExceedException { get; set; } = true;
        public bool OverlapException { get; set; } = true;

        public EncodablePropertyAttribute(int start)
        {
            Start = start;
            Count = 1;
        }
        
        public EncodablePropertyAttribute(int start, int count)
        {
            Start = start;
            Count = count;
        }
    }
}