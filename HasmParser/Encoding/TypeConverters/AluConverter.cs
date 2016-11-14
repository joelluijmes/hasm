using System;
using System.ComponentModel;
using System.Globalization;
using hasm.Parsing.Models;

namespace hasm.Parsing.Encoding.TypeConverters
{
    internal sealed class AluConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is ALU == false)
                return base.ConvertFrom(context, culture, value);

            var alu = (ALU)value;
            return PropertyEncoder.Encode(alu);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
            sourceType == typeof(ALU) || base.CanConvertFrom(context, sourceType);
    }
}