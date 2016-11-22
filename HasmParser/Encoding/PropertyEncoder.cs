using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using hasm.Parsing.Encoding.TypeConverters;
using hasm.Parsing.Models;

namespace hasm.Parsing.Encoding
{
    public static class PropertyEncoder
    {
        private static readonly Dictionary<Type, TypeConverter> _converters = new Dictionary<Type, TypeConverter>();
        private static readonly Dictionary<Type, List<EncodableMember>> _cache = new Dictionary<Type, List<EncodableMember>>();

        public static long Encode(object obj)
        {
            var encodableMembers = GetEncodableMembers(obj);

            long total = 0;
            foreach (var member in encodableMembers)
            {
                var value = GetValue(obj, member);

                if (member.Encodable.ExceedException && (value > Math.Pow(2, member.Encodable.Count) - 1))
                    throw new NotImplementedException();

                total += value << member.Encodable.Start;
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
                    ? (long) (converter.ConvertFrom(aluContext, CultureInfo.InvariantCulture, objValue) ?? 0)
                    : (long) (converter.ConvertFrom(objValue) ?? 0))
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

            converter = (TypeConverter) Activator.CreateInstance(type);
            _converters[type] = converter;
            return converter;
        }

        private static List<EncodableMember> GetEncodableMembers(object obj)
        {
            var type = obj.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            List<EncodableMember> encodableMembers;
            if (_cache.TryGetValue(type, out encodableMembers))
                return encodableMembers;

            encodableMembers = type
                .GetProperties(flags).Cast<MemberInfo>()
                .Concat(type.GetFields(flags))
                .Where(mem => mem.CustomAttributes.Any())
                .Select(mem => new
                {
                    Member = mem,
                    Attributes = (EncodablePropertyAttribute[])mem.GetCustomAttributes(typeof(EncodablePropertyAttribute), true)
                })
                .SelectMany(mems => mems.Attributes.Select(mem => new EncodableMember {Member = mems.Member, Encodable = mem}))
                .Where(x => x.Encodable != null)
                .ToList();

            CheckOverlap(encodableMembers);

            _cache[type] = encodableMembers;
            return encodableMembers;
        }

        private static void CheckOverlap(IList<EncodableMember> encodableMembers)
        {
            var encodables = encodableMembers
                .SelectMany(x => encodableMembers, (x, y) => new { x, y })
                .Where(t => t.x != t.y);

            var list = new List<Tuple<MemberInfo, MemberInfo>>();
            foreach (var enc in encodables)
            {
                var x = enc.x.Encodable;
                var y = enc.y.Encodable;

                if ((x.OverlapException || y.OverlapException) &&
                    (x.Start < y.Start + y.Count) &&
                    (y.Start < x.Start + x.Count))
                    list.Add(Tuple.Create(enc.x.Member, enc.y.Member));
            }

            if (!list.Any())
                return;

            var message = list
                .Select(s => $"{s.Item1.Name} {s.Item2.Name}")
                .Aggregate("Overlaps between:", (a, b) => $"{a}\r\n {b}");

            throw new Exception(message);
        }

        private struct EncodableMember
        {
            public MemberInfo Member { get; set; }
            public EncodablePropertyAttribute Encodable { get; set; }

            public object GetValue(object obj)
            {
                if (Member.MemberType == MemberTypes.Property)
                    return ((PropertyInfo) Member).GetValue(obj);

                if (Member.MemberType == MemberTypes.Field)
                    return ((FieldInfo) Member).GetValue(obj);

                throw new InvalidOperationException();
            }

            private bool Equals(EncodableMember other) => Equals(Encodable, other.Encodable) && Equals(Member, other.Member);

            public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (obj is EncodableMember && Equals((EncodableMember) obj));

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Encodable?.GetHashCode() ?? 0)*397) ^ (Member != null
                               ? Member.GetHashCode()
                               : 0);
                }
            }

            public static bool operator ==(EncodableMember left, EncodableMember right) => left.Equals(right);

            public static bool operator !=(EncodableMember left, EncodableMember right) => !left.Equals(right);
        }
    }
}
