using System;
using System.ComponentModel;
using System.Globalization;
using hasm.Parsing.Models;

namespace hasm.Parsing.Encoding.TypeConverters
{
    internal sealed class RightConverter : TypeConverter
    {
        private const long RIGHT_DISABLED = 0x0F;
        private const long EXT_IMMEDIATE = 0x0A;
        private const long INT_IMMEDIATE = 0x09;

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var alu = context.Instance as ALU;
            if (value is OperandConverter == false || alu == null)
                return base.ConvertFrom(context, culture, value);

            var right = (OperandConverter)value;

            if (string.IsNullOrEmpty(right.Operand))
            {
                return alu.ExternalImmediate
                    ? EXT_IMMEDIATE
                    : RIGHT_DISABLED;
            }

            return right.IsImmediate
                ? INT_IMMEDIATE
                : right.Value;
        }
        
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
            sourceType == typeof(OperandConverter) || base.CanConvertFrom(context, sourceType);
    }
}