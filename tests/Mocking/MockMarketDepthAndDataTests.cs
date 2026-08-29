using System;
using System.Collections.Generic;
using NinjaTrader.UnitTest.Mocking;

namespace NinjaTrader.UnitTest
{
    public class MockMarketDepthAndDataTests : TestCase
    {
        public void TestMarketDepthLadderOperations()
        {
            var depth = new MockMarketDepth();

            // Insert Bids
            depth.ProcessDepth(MockMarketDataType.Bid, MockMarketDepthOperation.Insert, price: 5000.00, volume: 15);
            depth.ProcessDepth(MockMarketDataType.Bid, MockMarketDepthOperation.Insert, price: 4999.75, volume: 30);
            depth.ProcessDepth(MockMarketDataType.Bid, MockMarketDepthOperation.Insert, price: 5000.25, volume: 10);

            // Insert Asks
            depth.ProcessDepth(MockMarketDataType.Ask, MockMarketDepthOperation.Insert, price: 5000.75, volume: 20);
            depth.ProcessDepth(MockMarketDataType.Ask, MockMarketDepthOperation.Insert, price: 5000.50, volume: 25);

            // Best Bid should be highest (5000.25), Best Ask should be lowest (5000.50)
            AssertEqual(5000.25, depth.BestBid);
            AssertEqual(10, depth.BestBidVolume);
            AssertEqual(5000.50, depth.BestAsk);
            AssertEqual(25, depth.BestAskVolume);
            AssertAlmostEqual(0.25, depth.Spread, delta: 0.001);

            // Total volume
            AssertEqual(55, depth.TotalBidVolume);
            AssertEqual(45, depth.TotalAskVolume);

            // Update Best Ask volume
            depth.ProcessDepth(MockMarketDataType.Ask, MockMarketDepthOperation.Update, price: 5000.50, volume: 50, position: 0);
            AssertEqual(50, depth.BestAskVolume);

            // Remove Best Bid
            depth.ProcessDepth(MockMarketDataType.Bid, MockMarketDepthOperation.Remove, price: 5000.25, volume: 0, position: 0);
            AssertEqual(5000.00, depth.BestBid);
            AssertEqual(15, depth.BestBidVolume);
        }

        public void TestHarnessMarketEventsDispatch()
        {
            var harness = new NinjaScriptTestHarness();
            var receivedDataEvents = new List<MockMarketDataEventArgs>();
            var receivedDepthEvents = new List<MockMarketDepthEventArgs>();

            harness.OnMarketData(e => receivedDataEvents.Add(e));
            harness.OnMarketDepth(e => receivedDepthEvents.Add(e));

            harness.TriggerMarketData(MockMarketDataType.Last, price: 5002.50, volume: 5);
            harness.TriggerMarketDepth(MockMarketDataType.Bid, MockMarketDepthOperation.Insert, price: 5002.25, volume: 12);

            AssertEqual(1, receivedDataEvents.Count);
            AssertEqual(5002.50, receivedDataEvents[0].Price);
            AssertEqual(5, receivedDataEvents[0].Volume);

            AssertEqual(1, receivedDepthEvents.Count);
            AssertEqual(5002.25, receivedDepthEvents[0].Price);
            AssertEqual(5002.25, harness.MarketDepth.BestBid);
        }

        public void TestConnectionLifecycleEvents()
        {
            var connection = new MockConnection("RithmicLive");
            var statusEvents = new List<MockConnectionStatusEventArgs>();

            connection.ConnectionStatusChanged += (sender, e) => statusEvents.Add(e);

            connection.Connect();
            AssertEqual(MockConnectionStatus.Connected, connection.Status);

            connection.SimulateConnectionLoss("Internet disconnected");
            AssertEqual(MockConnectionStatus.ConnectionLost, connection.Status);

            AssertEqual(2, statusEvents.Count);
            AssertEqual(MockConnectionStatus.Connected, statusEvents[0].Status);
            AssertEqual(MockConnectionStatus.ConnectionLost, statusEvents[1].Status);
            AssertEqual("Internet disconnected", statusEvents[1].ErrorMessage);
        }
    }
}
