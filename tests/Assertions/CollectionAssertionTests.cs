using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.UnitTest;

namespace NinjaTrader.UnitTest.Tests.Assertions
{
    public class CollectionAssertionTests : TestCase
    {
        public void TestAssertInAndNotInWithVariousCollections()
        {
            var list = new List<int> { 1, 2, 3, 5, 8 };
            AssertIn(3, list);
            AssertNotIn(4, list);

            var array = new string[] { "ES", "NQ", "YM", "RTY" };
            AssertIn("ES", array);
            AssertNotIn("CL", array);

            var hashSet = new HashSet<string> { "AAPL", "MSFT", "GOOG" };
            AssertIn("AAPL", hashSet);
            AssertNotIn("TSLA", hashSet);

            var linqSeq = list.Where(x => x > 2);
            AssertIn(5, linqSeq);
            AssertNotIn(1, linqSeq);

            // Aliases
            Contains("MSFT", hashSet);
            DoesNotContain("AMZN", hashSet);
        }

        public void TestSequenceAndCountEqual()
        {
            var seq1 = new List<int> { 1, 2, 3, 4 };
            var seq2 = new int[] { 1, 2, 3, 4 };
            var seq3 = new List<int> { 4, 3, 2, 1 };

            AssertSequenceEqual(seq1, seq2);
            AssertCountEqual(seq1, seq3);

            var emptyList = new List<string>();
            var populatedList = new List<string> { "data" };

            AssertEmpty(emptyList);
            AssertNotEmpty(populatedList);

            // Aliases
            IsEmpty(emptyList);
            IsNotEmpty(populatedList);
        }
    }
}
