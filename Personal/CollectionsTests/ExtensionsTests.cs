using System;
using Xunit;
using Collections;
using System.Linq;
using System.Collections.Generic;

namespace CollectionsTests
{
    public class ExtensionsTests
    {
        public Comparison<int> compareInts = (a, b) => a - b;

        [Fact]
        public void TestAllAscending()
        {
            var array = Enumerable.Range(0, 10).ToArray();

            SortAndCheckArrays(compareInts, array);
        }

        [Fact]
        public void TestAllDescending()
        {
            var array = Enumerable.Range(0, 10).Reverse().ToArray();

            SortAndCheckArrays(compareInts, array);
        }

        [Fact]
        public void TestSingleBitonicSequence()
        {
            const int length = 10;
            for (var i = 0; i < length; i++)
            {
                var array = CreateBitonicSequence(length, i).ToArray();

                SortAndCheckArrays(compareInts, array);
            }
        }

        private static IEnumerable<int> CreateBitonicSequence(int length, int midpoint)
        {
            return Enumerable.Range(0, midpoint).Concat(Enumerable.Range(midpoint, length).Reverse());
        }

        [Fact]
        public void TestMultipleBitonicSequences()
        {
            for (var i = 0; i < 10; i++)
            {
                var array = new List<int>();

                for (var j = 0; j < 10; j++)
                {
                    array.AddRange(CreateBitonicSequence(10, j));
                }

                SortAndCheckArrays(compareInts, array.ToArray());
            }
        }

        [Fact]
        public void TestNaturalSortPermutations()
        {
            const int count = 9;
            for (var i = 0; i < count; i++)
            {
                var original = new int[i];
                var copy1 = new int[i];
                var copy2 = new int[i];

                do
                {
                    SortAndCheckArrays(compareInts, original, copy1, copy2);
                }
                while (Increment(original, i - 1));
            }
        }

        private void SortAndCheckArrays(Comparison<int> compare, int[] original, int[] copy1 = null, int[] copy2 = null)
        {
            copy1 = copy1 ?? new int[original.Length];
            copy2 = copy2 ?? new int[original.Length];

            Array.Copy(original, copy1, original.Length);
            Array.Copy(original, copy2, original.Length);

            copy1.NaturalMergeSort(compare);
            Array.Sort(copy2, compare);

            AssertEquals(copy1, copy2);
        }

        private void AssertEquals(int[] copy1, int[] copy2)
        {
            for (var i = 0; i < copy1.Length; i++)
            {
                if (copy1[i] != copy2[i])
                {
                    Assert.True(false);
                }
            }
        }

        private bool Increment(int[] original, int count)
        {
            for (var i = 0; i < original.Length; i++)
            {
                if (original[i] == count)
                {
                    if (i == original.Length - 1)
                    {
                        return false;
                    }
                    original[i] = 0;
                    continue;
                }

                original[i]++;
                return true;
            }

            return false;
        }
    }
}
