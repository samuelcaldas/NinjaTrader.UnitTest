using System;
using NinjaTrader.UnitTest.Mocking;

namespace NinjaTrader.UnitTest
{
    public class MockLevel2AnalyticsTests : TestCase
    {
        private MockMarketDepth _depth;

        public override void SetUp()
        {
            _depth = new MockMarketDepth();

            // Populate Bid Ladder (5000.00, 4999.75, 4999.50, 4999.25, 4999.00)
            _depth.ProcessDepth(MockMarketDataType.Bid, MockMarketDepthOperation.Insert, 5000.00, 30);
            _depth.ProcessDepth(MockMarketDataType.Bid, MockMarketDepthOperation.Insert, 4999.75, 40);
            _depth.ProcessDepth(MockMarketDataType.Bid, MockMarketDepthOperation.Insert, 4999.50, 50);

            // Populate Ask Ladder (5000.25, 5000.50, 5000.75)
            _depth.ProcessDepth(MockMarketDataType.Ask, MockMarketDepthOperation.Insert, 5000.25, 10);
            _depth.ProcessDepth(MockMarketDataType.Ask, MockMarketDepthOperation.Insert, 5000.50, 20);
            _depth.ProcessDepth(MockMarketDataType.Ask, MockMarketDepthOperation.Insert, 5000.75, 30);
        }

        public void TestOrderBookImbalanceCalculation()
        {
            // Top 3 Bids: 30 + 40 + 50 = 120
            // Top 3 Asks: 10 + 20 + 30 = 60
            // Total: 180
            // Imbalance: 120 / 180 = 0.6667 (66.7% Bids)
            AssertEqual(120, _depth.GetCumulativeBidVolume(3));
            AssertEqual(60, _depth.GetCumulativeAskVolume(3));
            AssertAlmostEqual(0.6667, _depth.GetBidAskImbalance(3), delta: 0.001);
        }

        public void TestDepthSnapshotCapture()
        {
            var snapshot = _depth.TakeSnapshot();

            AssertEqual(5000.00, snapshot.BestBid);
            AssertEqual(5000.25, snapshot.BestAsk);
            AssertEqual(3, snapshot.Bids.Count);
            AssertEqual(3, snapshot.Asks.Count);
            AssertAlmostEqual(0.25, snapshot.Spread, delta: 0.001);
        }

        public void TestVolumeProfileAndPOC()
        {
            // Record trades
            _depth.RecordTrade(5000.25, volume: 100, isAggressiveBuy: true);
            _depth.RecordTrade(5000.50, volume: 300, isAggressiveBuy: true); // Heaviest volume (POC)
            _depth.RecordTrade(5000.00, volume: 80, isAggressiveBuy: false);

            var profile = _depth.VolumeProfile;

            AssertEqual(480, profile.TotalVolume);
            AssertEqual(320, profile.CumulativeDelta); // 400 Buy - 80 Sell = +320 Delta
            AssertEqual(5000.50, profile.PointOfControl);

            var node = profile.GetNodeAt(5000.50);
            AssertEqual(300, node.TotalVolume);
            AssertEqual(300, node.BuyVolume);
            AssertEqual(0, node.SellVolume);
        }
    }
}
