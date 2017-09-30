using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable PossibleMultipleEnumeration

namespace Paycor.Import.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool ContainDuplicates<T, T1>(this IEnumerable<T> input, Func<T, T1> selector, IEqualityComparer<T1> comparer)
        {
            Ensure.ThatArgumentIsNotNull(input, nameof(input));
            Ensure.ThatArgumentIsNotNull(selector, nameof(selector));
            Ensure.ThatArgumentIsNotNull(comparer, nameof(comparer));

            var set = new HashSet<T1>(comparer);

            foreach (var item in input)
            {
                if (!set.Add(selector(item)) && !selector(item).ToString().ContainsOpenAndClosedBraces())
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<IEnumerable<T>> SplitItemsList<T>(this IEnumerable<T> input, int splitSize)
        {
            if (input == null)
                return null;

            var itemsList = new List<List<T>>();

            while (input.Any())
            {
                itemsList.Add(input.Take(splitSize).ToList());
                input = input.Skip(splitSize).ToList();
            }
            return itemsList;
        }
    }
}