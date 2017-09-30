using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Paycor.Import.Registration
{
    [ExcludeFromCodeCoverage]
    public static class Combinations
    {
        public static List<List<T>> AllCombinationsOf<T>(List<List<T>> sets)
        {
            if (sets == null) return null;
            if (sets.Count <= 0) return null;

            var combinations = sets[0].Select(value => new List<T> {value}).ToList();

            return sets.Skip(1).Aggregate(combinations, AddExtraSet);
        }


        private static List<List<T>> AddExtraSet<T>
            (List<List<T>> combinations, List<T> set)
        {
            return   (from value in set
                      from combination in combinations
                      select new List<T>(combination) { value }).ToList();
        }
    }
}
