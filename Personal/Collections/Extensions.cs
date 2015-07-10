using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Collections
{
    public static class Extensions
    {
        private enum SortAnalysis
        {
            SourceFullySorted,
            DestinationFullySorted,
            NeitherFullySorted
        }

        [DebuggerDisplay("{DebuggerDisplay}")]
        private struct BitonicSequence
        {
            public readonly int LeftIndexInclusive;
            public readonly int HighPointIndexInclusive;
            public readonly int RightIndexInclusive;

            public BitonicSequence(int leftIndexInclusive, int highPointIndexInclusive, int rightIndexInclusive)
            {
                this.LeftIndexInclusive = leftIndexInclusive;
                this.RightIndexInclusive = rightIndexInclusive;
                this.HighPointIndexInclusive = highPointIndexInclusive;
            }

            private string DebuggerDisplay => $"({LeftIndexInclusive},{HighPointIndexInclusive},{RightIndexInclusive})";

            //internal BitonicSequence Order(bool ascending)
            //{
            //    return ascending
            //        ? new BitonicSequence(this.LeftIndexInclusive, this.RightIndexInclusive, this.RightIndexInclusive)
            //        : new BitonicSequence(this.LeftIndexInclusive, this.LeftIndexInclusive, this.RightIndexInclusive);
            //}
        }

        /// <summary>
        /// Performs a 'natural' merge sort on the given array.  A 'natural' merge sort is an 
        /// 'adaptive' version of the merge sort algorithm.  i.e. it is a sort that performs
        /// better than the worst case O(n * log(n)) number of comparisons the closer the input
        /// is to being fully sorted.  In the best case (when the input is already sorted),
        /// this algorithm only performs 'n' comparisons in total.
        /// 
        /// The actual running complexity of this algorithm is O(n * log(s)) where 's' is number
        /// of 'bitonic sequences' in the input.  A 'bitonic sequence' is a subsequence of the 
        /// input that is monotonically increasing to a certain point, and then is monotonically 
        /// decreasing.  For example, the sequence '1, 2, 4, 8, 7, 5, 0' is bitonic.  The 
        /// monotonically increasing sequence is '1, 2, 4, 8' and the monotonically decreasing 
        /// sequence is '8, 7, 5, 0'.
        /// 
        /// So, in practice, if your input has a few sequences of ascending/descending values, then
        /// this sort performs extremely well (near-linear).  In the worst case (an input that
        /// ascends/descends with each pair of values), then the number of bitonic sequences will be 
        /// n/2 and the running time will be n*log(n) (i.e. the same as the normal introspective
        /// sort provided by the runtime).
        /// 
        /// O(n) additional space is used by an auxilliary storage array.  However, this array is
        /// pooled so as to not cause too much garbage pressure on the system.
        /// </summary>
        /// 
        /// The algorithm for this sort is extremely simple.  We'll start with the base case that 
        /// you have an input with a single bitonic sequence in it.  For the sake of example
        /// consider that sequence to be:
        /// 
        ///     2, 4, 6, 5, 3.
        /// 
        /// This bitonic sequence is exactly two ascending sequences.  One sequence ascending from
        /// the left side "2, 4, 6", and one sequence ascending from the right "3, 5, 6".  By walking
        /// inwards from both sides of this sequence, we can trivially produce the sorted sequence of
        /// all these values in our temp storage in the same location.  i.e. we'll compare '2' to '3',
        /// and place '2' into the start of this same range in our temp storage.  We'll then compare
        /// '3' to '4' and place '3' into the next storage location, then '4' to '5' and so on.  
        ///
        /// We'll now have a completed sorted sequence and our base-case is complete.  Finding a 
        /// bitonic sequence can be done in linear time, and sorting that sequence can be done in 
        /// linear time as well.
        /// 
        /// Now we'll do the inductive step.  Saw we have two bitonic sequences.  For example:
        /// 
        ///     2, 4, 6, 5, 3, 6, 7, 8, 9, 2, 1
        /// 
        /// The two bitonic sequences are "2, 4, 6, 5, 3" and "6, 7, 8, 9, 2, 1".  For each pair of 
        /// bitonic sequences, we'll sort the first part of the pair into in ascending fashion, and
        /// we'll sort the second part of the pair in descending fashion. So, with the above example
        /// we'll sort teh first pair into:
        /// 
        ///     2, 3, 4, 5, 6       and the second into    9, 8, 7, 6, 2, 1     producing the final sequence:
        /// 
        ///     2, 3, 4, 5, 6, 9, 8, 7, 6, 2, 1
        /// 
        /// Because we concatenated an ascending sequence with a descending sequence, we have made a
        /// new bitonic sequence.   So for all pairs of bitonic sequences, we'll produce a single 
        /// bitonic sequence (thus halving the number of bitonic sequences).  We will repeat this 
        /// until we have a single bitonic sequence (which gives us our base case).  Because we are 
        /// halving the number of sequences with each pass we do, we'll need to do log(s) passes.
        /// 
        /// Each pass will be done in linear time.  Leaving us with n*log(s) as the expected number
        /// of comparisons we'll need to do.
        public static void NaturalMergeSort<T>(this T[] values, Comparison<T> compare)
        {
            var length = values.Length;
            if (length <= 1)
            {
                return;
            }

            var tempStorage = new T[length];

            var source = values;
            var destination = tempStorage;

            var sourceBitonicSequences = new List<BitonicSequence>();
            var destinationBitonicSequences = new List<BitonicSequence>();

            ComputeBitonicSequences(source, compare, sourceBitonicSequences);

            // As long as their are multiple bitonic sequences, keep taking pairs of them to
            // form new, larger bitonic sequences.  Each time through this loop this will cause
            // us to at least halve the number of sequences we have.  So there will be log(s) 
            // iterations of this loop in the worst case.
            //
            // Note: we'll try to be smarter than this in some cases as well.  If a subsequent
            // bitonic sequence is entire above or below the sequence we're looking at, then
            // we can create one long ascending or descending run without having to compare
            // all the constituent elements of both sequences.   i.e. if we have:
            //
            //      1, 3, 5, 4, 2     and     6, 8, 10, 9, 7, 
            //
            // then we can trivially determine that the second sequence is above the first 
            // (since we know the high and low values of a bitonic sequence in constant time).
            // As such, instead of producing and ascending sequence followed by a descending
            // one, i.e.:
            //
            //      1, 2, 3, 4, 5, 10, 9, 8, 7, 6
            //
            // We instead produce the ascending list for both, producing:
            //
            //      1, 2, 3, 4, 5, 6, 7, 8, 9, 10
            while (sourceBitonicSequences.Count >= 2)
            {
                destinationBitonicSequences.Clear();

                CombineSuccessiveSequences(compare, source, destination, sourceBitonicSequences, destinationBitonicSequences);

                // Swap where we're moving things, and keep going.
                Swap(ref source, ref destination);
                Swap(ref sourceBitonicSequences, ref destinationBitonicSequences);
            }

            // We're down to a single bitonic sequence. If it is already sorted, then we only need
            // to copy those sorted elements back into the 'values' array.
            var sequence = sourceBitonicSequences[0];
            if (IsSortedAscending(sequence))
            {
                Copy(source, values);
            }
            else
            {
                // Otherwise, do the final sort and copy the result to the 'values' array.
                MergeSort(source, destination, sequence, compare, ascending: true);
                Copy(destination, values);
            }
        }

        private static bool IsSortedAscending(BitonicSequence sequence)
        {
            return sequence.HighPointIndexInclusive == sequence.RightIndexInclusive;
        }

        private static void Swap<T>(ref T source, ref T destination)
        {
            var temp = source;
            source = destination;
            destination = temp;
        }

        private static void CombineSuccessiveSequences<T>(Comparison<T> compare, T[] source, T[] destination, List<BitonicSequence> sourceBitonicSequences, List<BitonicSequence> destinationBitonicSequences)
        {
            var bitonicSequenceCount = sourceBitonicSequences.Count;

            // Jump through our existing bitonic sequences two at a time.
            for (var currentSequenceIndex = 0; currentSequenceIndex < bitonicSequenceCount; currentSequenceIndex += 2)
            {
                var currentSequence = sourceBitonicSequences[currentSequenceIndex];

                if (currentSequenceIndex == bitonicSequenceCount - 1)
                {
                    // We're on the last bitonic sequence.  Merge it into destination in the 
                    // correct direction.
                    MergeSort(source, destination, currentSequence, compare, ascending: true);
                    destinationBitonicSequences.Add(new BitonicSequence(
                        currentSequence.LeftIndexInclusive, currentSequence.RightIndexInclusive, currentSequence.RightIndexInclusive));
                    break;
                }

                // First merge sort the current bitonic sequence and place into 'destination'
                // in the right order.
                var nextSequence = sourceBitonicSequences[currentSequenceIndex + 1];
                MergeSort(source, destination, currentSequence, compare, ascending: true);
                MergeSort(source, destination, nextSequence, compare, ascending: false);

                // The highpoint will either be the right side of the ascending sequence, or 
                // the left side of the descending sequence (whichever is has the greater value
                // at that index).
                var highPoint = compare(destination[currentSequence.RightIndexInclusive], destination[nextSequence.LeftIndexInclusive]) >= 0
                    ? currentSequence.RightIndexInclusive
                    : nextSequence.LeftIndexInclusive;
                destinationBitonicSequences.Add(new BitonicSequence(
                    currentSequence.LeftIndexInclusive, highPoint, nextSequence.RightIndexInclusive));
            }
        }

        private static void ComputeBitonicSequences<T>(T[] values, Comparison<T> compare, List<BitonicSequence> bitonicSequences)
        {
            var length = values.Length;

            // The left side (inclusive) of the current bitonic sequence.
            var left = 0;

            // The right side (exclusive) of the current bitonic sequence.
            var right = 0;

            // Find successive bitonic sequences.  The first bitonic sequence we see, we'll merge 
            // into 'destination' in ascending order.  The second bitonic sequene we see, we'll 
            // merge into 'destination' descending order.  This will procude a new bitonic sequence
            // that will get taken care of in the next iteration of the top level NaturalMergeSort
            // loop.
            //
            // Then the third sequence gets merged in ascending order, then the fourth in descending 
            // order.  And so on and so forth until we've processed all the bitonic sequences in 'source'.
            while (right < length)
            {
                // Mark the start of the current bitonic sequence.
                left = right;

                // First find the longest ascending sequence.
                T temp;
                do
                {
                    temp = values[right];
                    right++;
                }
                while (right < length && compare(temp, values[right]) <= 0);

                // The index of the highest value in the bitonic sequence.  All values from 
                // [left, highPoint) will be less than or equal to this value, and will be in 
                // ascending order.  All values  from (highPoint, right] will be less than or 
                // equal to this value, and will be in descending order.
                var highPoint = right - 1;

                // Now the longest descending sequence.
                while (right < length && compare(temp, values[right]) >= 0)
                {
                    temp = values[right];
                    right++;
                }

                // Note: in 'BitonicSequence' 'right' is *inclusive*, so we subtract '1' here.
                bitonicSequences.Add(new BitonicSequence(left, highPoint, right - 1));
            }
        }

        private static void Copy<T>(T[] from, T[] to)
        {
            if (from == to)
            {
                // No need to copy if the destination is the same as the source.
                // TODO(cyrusn): Find out if this optimization is needed.  Does the BCL already do this?
                return;
            }

            Array.Copy(sourceArray: from, destinationArray: to, length: to.Length);
        }

        private static void MergeSort<T>(T[] source, T[] destination, BitonicSequence sequence, Comparison<T> compare, bool ascending)
        {
            // Merges a bitonic sequence from the [source_left, source_right] range into 
            // 'destination' (in the same [destination_left, destination_right] range).  The values
            // will be placed into 'destination' in either ascending order or descending order 
            // based on the value of 'ascending'.  So, for example, if we had the bitonic range:
            //
            //  ... 2, 4, 6, 8, 7, 5, 3, 1 ...  , and 'true' for 'ascending', we would end up  with:
            //
            //  ... 1, 2, 3, 4, 5, 6, 7, 8 ...
            //
            // If we had the same bitonic range, but 'false' for 'ascending', we would end up with:
            //
            //  ... 8, 7, 6, 5, 4, 3, 2, 1 ...
            //
            //
            // *** For the purposes of simplification, all further explanation will assume that
            // *** 'ascending' is true. 
            // 
            //
            // Because the sequence is bitonic, we can easily produce the final sorted result.  
            // Each side of the bitonic sequence is a sorted array.  i.e. [2, 4, 6, 8] and 
            // [1, 3, 5, 7, 8].  So we do a simple merge, taking the lowest from either side
            // and continuing to walk up both sides until we we have consumed all the elements.
            //
            // 
            // We can also optimize this further once we exhaust either side of the bitonic sequence.
            // For example, say our bitonic sequence is:
            //
            //      3, 4, 5, 6, 7, 8, 9, 2, 1.
            //
            // The top-point of the bitonic sequence would be the '9' value.  We'll start walking
            // up both sides, merging the smallest into 'destination'.  This means we'll add '1' 
            // and then '2' to the destination array. Once we've reached the top-point on the right
            // side, we know we don't have to do any more comparisons.  We can simply walk the
            // left side until we hit the midpoint since we know it is already sorted and we know
            // there aren't any more values that could be lower.

            var left = sequence.LeftIndexInclusive;
            var highPoint = sequence.HighPointIndexInclusive;
            var right = sequence.RightIndexInclusive;

            int destinationIndex = ascending ? left : right;
            int increment = ascending ? 1 : -1;

            while (left <= right)
            {
                if (left == highPoint)
                {
                    // We've consumed all the values on the left of the bitonic sequence.  All the 
                    // remainder values are on the right side and are sorted in descending order.
                    MergeRightSideOfBitonicSequence(source, destination, left, right, destinationIndex, increment);
                    return;
                }
                else if (right == highPoint)
                {
                    // We've consumed all the values on the right side of the bitonic sequence. All
                    // the remainder values are on the left sdie and are sorted in ascending order.
                    MergeLeftSideOfBitonicSequence(source, destination, left, right, destinationIndex, increment);
                    return;
                }

                // Either side of the bitonic range might be the smaller value.  Find out which is
                // smaller and place that into the destination array in the right location.
                if (compare(source[left], source[right]) <= 0)
                {
                    destination[destinationIndex] = source[left];
                    left++;
                }
                else
                {
                    destination[destinationIndex] = source[right];
                    right--;
                }

                destinationIndex += increment;
            }
        }

        private static void MergeLeftSideOfBitonicSequence<T>(T[] source, T[] destination, int left, int right, int destinationIndex, int increment)
        {
            // TODO(cyrusn): We could optimize this as an Array.Copy if increment is +1.  In that 
            // case we're producing an ascending list (and we know we're already ascending because
            // we're the left side of the sequence).

            while (left <= right)
            {
                destination[destinationIndex] = source[left];
                left++;
                destinationIndex += increment;
            }
        }

        private static void MergeRightSideOfBitonicSequence<T>(T[] source, T[] destination, int left, int right, int destinationIndex, int increment)
        {
            // TODO(cyrusn): We could optimize this as an Array.Copy if increment is -1.  in that
            // case we're producing a descending list (and we know we're already descending because
            // we're the right side of the sequence).

            while (left <= right)
            {
                destination[destinationIndex] = source[right];
                right--;
                destinationIndex += increment;
            }
        }
    }
}
