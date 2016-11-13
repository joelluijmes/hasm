using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace hasm.Parsing.Encoding
{
    public static class PropertyEncoder
    {
        public static long Encode(object obj)
        {
            var type = obj.GetType();
            var properties = type
                .GetProperties()
                .Select(prop => new
                {
                    Property = prop,
                    Encodable = (EncodablePropertyAttribute)prop.GetCustomAttribute(typeof(EncodablePropertyAttribute), true)
                })
                .Where(x => x.Encodable != null)
                .ToList();

            var query = from x in properties
                        from y in properties
                        where x != y &&
                        (x.Encodable.OverlapException || y.Encodable.OverlapException) &&
                        x.Encodable.Start < (y.Encodable.Start + y.Encodable.Count) &&
                        y.Encodable.Start < (x.Encodable.Start + x.Encodable.Count)
                        select new {X = x, Y = y};

            var overlaps = query.ToList();
            if (overlaps.Any())
            {
                var message = overlaps
                    .Select(s => $"{s.X.Property.Name} {s.Y.Property.Name}")
                    .Aggregate("Overlaps between:", (a, b) => $"{a}\r\n {b}");

                throw new Exception(message);
            }

            var converter = TypeDescriptor.GetConverter(typeof(long));
            long total = 0;
            foreach (var x in properties)
            {
                var objValue = x.Property.GetValue(obj);
                var value = converter.CanConvertFrom(objValue.GetType())
                    ? (long) converter.ConvertFrom(objValue)
                    : Convert.ToInt64(objValue);

                if (x.Encodable.ExceedException && value > Math.Pow(2, x.Encodable.Count) - 1)
                    throw new NotImplementedException();
                
                total += value << x.Encodable.Start;
            }

            return total;
        }
    }
}