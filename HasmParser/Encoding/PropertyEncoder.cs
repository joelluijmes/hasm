using hasm.Parsing.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using hasm.Parsing.Encoding.TypeConverters;

namespace hasm.Parsing.Encoding
{
    public static class PropertyEncoder
    {
        private static readonly Dictionary<Type, TypeConverter> _converters = new Dictionary<Type, TypeConverter>();

        public static long Encode(object obj)
        {
            var encodableMembers = GetEncodableMembers(obj);
            
            long total = 0;
            foreach (var x in encodableMembers)
            {
                var value = GetValue(obj, x);

                if (x.Encodable.ExceedException && value > Math.Pow(2, x.Encodable.Count) - 1)
                    throw new NotImplementedException();
                
                total += value << x.Encodable.Start;
            }

            return total;
        }

        private static long GetValue(object obj, EncodableMember member)
        {
            var objValue = member.GetValue(obj);
            
            AluContext aluContext = null;
            var alu = obj as ALU;
            if (alu != null)
                aluContext = new AluContext(alu);
            
            var converter = GetConverter(member.Encodable) ?? TypeDescriptor.GetConverter(typeof(long));

            var value = converter.CanConvertFrom(objValue.GetType())
                ? (aluContext != null
                    ? (long) converter.ConvertFrom(aluContext, CultureInfo.InvariantCulture, objValue)
                    : (long) converter.ConvertFrom(objValue))
                : Convert.ToInt64(objValue);

            return value;
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
                .Concat(type.GetFields(flags))
                .Select(prop => new EncodableMember
                {
                    Member = prop,
                    Encodable = (EncodablePropertyAttribute[])prop.GetCustomAttributes(typeof(EncodablePropertyAttribute), true)
                })
                .Where(x => x.Encodable != null)
                .ToList();

            CheckOverlap(encodableMembers);
            return encodableMembers;
        }

        private static void CheckOverlap(IList<EncodableMember> encodableMembers)
        {
            var encodables = encodableMembers
                .SelectMany(x => encodableMembers, (x, y) => new {x, y})
                .Where(t => t.x != t.y);

            var list = new List<Tuple<MemberInfo, MemberInfo>();
            foreach (var enc in encodables)
            {
                var attributes = enc.x.Encodable
                    .SelectMany(a => enc.y.Encodable)
                    .Where(c => enc.x != enc.y);

                foreach (var attr in attributes)
                {
                    
                }
            }

            var query = encodableMembers.SelectMany(x => encodableMembers, (x, y) => new {x, y}).Where(@t => @t.x.Member != @t.y.Member &&
                                                                                                             (@t.x.Encodable.OverlapException || @t.y.Encodable.OverlapException) &&
                                                                                                             @t.x.Encodable.Start < (@t.y.Encodable.Start + @t.y.Encodable.Count) &&
                                                                                                             @t.y.Encodable.Start < (@t.x.Encodable.Start + @t.x.Encodable.Count)).Select(@t => new {X = @t.x, Y = @t.y});

            var overlaps = query.ToList();
            if (overlaps.Any())
            {
                var message = overlaps
                    .Select(s => $"{s.X.Member.Name} {s.Y.Member.Name}")
                    .Aggregate("Overlaps between:", (a, b) => $"{a}\r\n {b}");

                throw new Exception(message);
            }
        }

        private struct EncodableMember
        {
            public MemberInfo Member { get; set; }
            public EncodablePropertyAttribute[] Encodable { get; set; }

            public object GetValue(object obj)
            {
                if (Member.MemberType == MemberTypes.Property)
                    return ((PropertyInfo)Member).GetValue(obj);

                if (Member.MemberType == MemberTypes.Field)
                    return ((FieldInfo)Member).GetValue(obj);

                throw new InvalidOperationException();
            }

            public bool Equals(EncodableMember other)
            {
                return Equals(Encodable, other.Encodable) && Equals(Member, other.Member);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                return obj is EncodableMember && Equals((EncodableMember) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Encodable?.GetHashCode() ?? 0)*397) ^ (Member != null ? Member.GetHashCode() : 0);
                }
            }

            public static bool operator ==(EncodableMember left, EncodableMember right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(EncodableMember left, EncodableMember right)
            {
                return !left.Equals(right);
            }
        }
    }
}