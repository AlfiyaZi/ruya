using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruya.Core
{
    public static class EnumerableHelper
    {
        private static readonly Random Randomizer = new Random();

        // TEST method GetDuplicates
        // COMMENT method GetDuplicates
        public static IEnumerable<T> GetDuplicates<T>(this IEnumerable<T> source, bool distinct)
        {
            IEnumerable<T> result = source.GroupBy(item => item)
                                          .SelectMany(grp => grp.Skip(1));

            return distinct
                       ? result.Distinct()
                       : result;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            for (var counter = 0; counter < list.Count; counter++)
            {
                list.Swap(counter, Randomizer.Next(counter, list.Count));
            }
        }

        /// <exception cref="ArgumentOutOfRangeException">firtsElement||secondlElement >= list.Count()</exception>
        public static void Swap<T>(this IList<T> list, int firtsElement, int secondElement)
        {
            if (firtsElement >= list.Count())
            {
                throw new ArgumentOutOfRangeException(nameof(firtsElement));
            }
            if (secondElement >= list.Count())
            {
                throw new ArgumentOutOfRangeException(nameof(secondElement));
            }
            T temp = list[firtsElement];
            list[firtsElement] = list[secondElement];
            list[secondElement] = temp;
        }
    }
}