using System;
using System.ComponentModel;
using System.Globalization;
using hasm.Parsing.Models;

namespace hasm.Parsing.Encoding.TypeConverters
{
    internal sealed class ImmediateConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var alu = context.Instance as Operation;
            if ((value is OperandConverter == false) || (alu == null))
                return base.ConvertFrom(context, culture, value);

            var right = (OperandConverter) value;
            return !string.IsNullOrEmpty(right.Operand) && right.IsImmediate && !(alu.ExternalRight && right.IsImmediate)
                ? right.Value
                : 0;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
            (sourceType == typeof(OperandConverter)) || base.CanConvertFrom(context, sourceType);
    }
}
