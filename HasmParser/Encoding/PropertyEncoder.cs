using hasm.Parsing.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace hasm.Parsing.Encoding
{
    public static class PropertyEncoder
    {
        private static readonly Dictionary<Type, TypeConverter> _converters = new Dictionary<Type, TypeConverter>();

        public static long Encode(object obj)
        {
            var encodableMembers = GetEncodableMembers(obj);
            var alu = obj as ALU;
            var aluContext = new AluContext(alu,)
            
            long total = 0;
            foreach (var x in encodableMembers)
            {
                var converter = GetConverter(x.Encodable) ?? TypeDescriptor.GetConverter(typeof(long));

                var objValue = x.GetValue(obj);

                var value = converter.CanConvertFrom(objValue.GetType())
                    ? (long) converter.ConvertFrom(objValue)
                    : Convert.ToInt64(objValue);

                if (x.Encodable.ExceedException && value > Math.Pow(2, x.Encodable.Count) - 1)
                    throw new NotImplementedException();
                
                total += value << x.Encodable.Start;
            }

            return total;
        }

        private static TypeConverter GetConverter(EncodablePropertyAttribute encodable)
        {
            var type = encodable.Converter;
            if (type == null)
                return null;

            TypeConverter converter;
            if (_converters.TryGetValue(type, out converter))
                return converter;

            converter = (TypeConverter)Activator.CreateInstance(type);
            _converters[type] = converter;
            return converter;
        }

        private static List<EncodableMember> GetEncodableMembers(object obj)
        {
            var type = obj.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var encodableMembers = type
                .GetProperties(flags).Cast<MemberInfo>()
                .Concat(type.GetFields(flags).Cast<MemberInfo>())
                .Select(prop => new EncodableMember
                {
                    Member = prop,
                    Encodable = (EncodablePropertyAttribute)prop.GetCustomAttribute(typeof(EncodablePropertyAttribute), true)
                })
                .Where(x => x.Encodable != null)
                .ToList();

            var query = from x in encodableMembers
                        from y in encodableMembers
                        where x.Member != y.Member &&
                        (x.Encodable.OverlapException || y.Encodable.OverlapException) &&
                        x.Encodable.Start < (y.Encodable.Start + y.Encodable.Count) &&
                        y.Encodable.Start < (x.Encodable.Start + x.Encodable.Count)
                        select new { X = x, Y = y };

            var overlaps = query.ToList();
            if (overlaps.Any())
            {
                var message = overlaps
                    .Select(s => $"{s.X.Member.Name} {s.Y.Member.Name}")
                    .Aggregate("Overlaps between:", (a, b) => $"{a}\r\n {b}");

                throw new Exception(message);
            }

            return encodableMembers;
        }

        private struct EncodableMember
        {
            public MemberInfo Member { get; set; }
            public EncodablePropertyAttribute Encodable { get; set; }

            public object GetValue(object obj)
            {
                if (Member.MemberType == MemberTypes.Property)
                    return ((PropertyInfo)Member).GetValue(obj);
                else if (Member.MemberType == MemberTypes.Field)
                    return ((FieldInfo)Member).GetValue(obj);
                else
                    throw new InvalidOperationException();
            }
        }
    }
}