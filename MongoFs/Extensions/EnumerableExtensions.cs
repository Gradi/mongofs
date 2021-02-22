using System.Collections.Generic;

namespace MongoFs.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(long Index, T Item)> WithIndexes<T>(this IEnumerable<T> enumerable)
        {
            long index = 0;
            foreach (T item in enumerable)
            {
                yield return (index, item);
                index += 1;
            }
        }

        public static IEnumerable<T> Untuple<T>(this IEnumerable<(T, T)> enumerable)
        {
            foreach ((T, T) tuple in enumerable)
            {
                yield return tuple.Item1;
                yield return tuple.Item2;
            }
        }
    }
}
