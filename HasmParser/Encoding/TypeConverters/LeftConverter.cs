using System;
using System.ComponentModel;
using System.Globalization;

namespace hasm.Parsing.Encoding.TypeConverters
{
    internal sealed class LeftConverter : TypeConverter
    {
        private const long LEFT_DISABLED = 0x0F;

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is OperandConverter == false)
                return base.ConvertFrom(context, culture, value);

            var target = (OperandConverter)value;
            return string.IsNullOrEmpty(target.Operand)
                ? LEFT_DISABLED
                : target.Value;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
            sourceType == typeof(OperandConverter) || base.CanConvertFrom(context, sourceType);
    }
}