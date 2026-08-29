using System;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Point-in-time immutable snapshot of Level 2 market depth books.
    /// </summary>
    public class MockDepthSnapshot
    {
        public DateTime Timestamp { get; }
        public IReadOnlyList<MockDepthLevel> Bids { get; }
        public IReadOnlyList<MockDepthLevel> Asks { get; }
        public double BestBid { get; }
        public double BestAsk { get; }
        public double Spread => (BestBid > 0 && BestAsk > 0) ? BestAsk - BestBid : 0.0;

        public MockDepthSnapshot(DateTime timestamp, IEnumerable<MockDepthLevel> bids, IEnumerable<MockDepthLevel> asks, double bestBid, double bestAsk)
        {
            Timestamp = timestamp;
            var bidList = new List<MockDepthLevel>();
            if (bids != null)
            {
                foreach (var b in bids)
                    bidList.Add(new MockDepthLevel(b.Price, b.Volume, b.MarketMaker));
            }
            Bids = bidList;

            var askList = new List<MockDepthLevel>();
            if (asks != null)
            {
                foreach (var a in asks)
                    askList.Add(new MockDepthLevel(a.Price, a.Volume, a.MarketMaker));
            }
            Asks = askList;

            BestBid = bestBid;
            BestAsk = bestAsk;
        }
    }
}
