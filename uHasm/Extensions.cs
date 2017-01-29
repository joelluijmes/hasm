using System.Collections.Generic;
using System.Linq;

namespace hasm
{
    internal static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item })
                );
        }

        public static int ConvertToInt(this IList<byte> array)
        {
            var result = 0;
            for (var i = 0; i < array.Count; i++)
                result |= array[i] << ((array.Count - i - 1) * 8);

            return result;
        }
    }
}