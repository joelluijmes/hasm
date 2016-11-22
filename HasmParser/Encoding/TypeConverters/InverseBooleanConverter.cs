using System;
using System.ComponentModel;
using System.Globalization;

namespace hasm.Parsing.Encoding.TypeConverters
{
    internal sealed class InverseBooleanConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is bool == false)
                return base.ConvertFrom(context, culture, value);

            var boolean = (bool)value;
            return boolean ? 0L : 1L;   
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
            sourceType == typeof(bool) || base.CanConvertFrom(context, sourceType);
    }
}