using System;
using Xunit;
using Collections;

namespace CollectionsTests
{
    public class ExtensionsTests
    {
        [Fact]
        public void TestNaturalSort()
        {
            Comparison<int> compare = (a, b) => a - b;
            const int count = 9;
            for (var i = 0; i < count; i++)
            {
                var original = new int[i];
                var copy1 = new int[i];
                var copy2 = new int[i];

                do
                {
                    Array.Copy(original, copy1, original.Length);
                    Array.Copy(original, copy2, original.Length);

                    copy1.NaturalMergeSort(compare);
                    Array.Sort(copy2, compare);

                    AssertEquals(copy1, copy2);
                }
                while (Increment(original, i - 1));
            }
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
