using System;

namespace hasm.Parsing.Encoding
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class EncodablePropertyAttribute : Attribute
    {
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

        public EncodablePropertyAttribute(Type converter)
        {
            Converter = converter;
        }

        public int Start { get; }
        public int Count { get; }
        public Type Converter { get; set; }
        public bool ExceedException { get; set; } = true;
        public bool OverlapException { get; set; } = true;
    }
}
