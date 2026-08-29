using System;
using System.Collections.Generic;
using NinjaTrader.UnitTest.Mocking;

namespace NinjaTrader.UnitTest
{
    public class MockMarketReplayTests : TestCase
    {
        public void TestParseTickTextData()
        {
            string tickCsv = @"
# NinjaTrader Tick Export
20260105 093000;5000.25;15
20260105 093001;5000.50;25
20260105 093002;5000.75;10
";
            var events = MarketReplayReader.ReadFromString(tickCsv);

            AssertEqual(3, events.Count);
            AssertEqual(MarketReplayRecordType.Tick, events[0].RecordType);
            AssertEqual(5000.25, events[0].Price);
            AssertEqual(15, events[0].Volume);
            AssertEqual(5000.50, events[1].Price);
            AssertEqual(25, events[1].Volume);
        }

        public void TestParseTickReplayTextData()
        {
            string tickReplayCsv = @"
20260105 093000 1000000;5000.25;5000.00;5000.25;5
20260105 093000 2000000;5000.25;5000.25;5000.50;12
";
            var events = MarketReplayReader.ReadFromString(tickReplayCsv);

            AssertEqual(2, events.Count);
            AssertEqual(MarketReplayRecordType.TickReplay, events[0].RecordType);
            AssertEqual(5000.25, events[0].Price);
            AssertEqual(5000.00, events[0].BidPrice);
            AssertEqual(5000.25, events[0].AskPrice);
            AssertEqual(5, events[0].Volume);
        }

        public void TestParseMarketDepthReplayTextData()
        {
            string depthCsv = @"
20260105 093000;Bid;Insert;5000.00;20;0;NSDQ
20260105 093000;Ask;Insert;5000.25;15;0;NSDQ
20260105 093001;Bid;Update;5000.00;35;0;NSDQ
";
            var events = MarketReplayReader.ParseDepthFromText(depthCsv);

            AssertEqual(3, events.Count);
            AssertEqual(MockMarketDataType.Bid, events[0].MarketDepth.MarketDataType);
            AssertEqual(MockMarketDepthOperation.Insert, events[0].MarketDepth.Operation);
            AssertEqual(5000.00, events[0].MarketDepth.Price);
            AssertEqual(20, events[0].MarketDepth.Volume);
            AssertEqual("NSDQ", events[0].MarketDepth.MarketMaker);
        }

        public void TestParseMinuteBarsTextData()
        {
            string minuteCsv = @"
20260105 093000;5000.00;5010.00;4995.00;5005.00;1200
20260105 093100;5005.00;5015.00;5002.00;5012.00;1500
";
            var bars = MarketReplayReader.ParseBarsFromText(minuteCsv, "ES", MockBarsPeriodType.Minute, 1);

            AssertEqual(2, bars.Count);
            AssertEqual(5005.00, bars.GetBarAt(0).Close);
            AssertEqual(5012.00, bars.GetBarAt(1).Close);
            AssertEqual(1200, bars.GetBarAt(0).Volume);
        }

        public void TestReplayBuilderAndPlayerPlayback()
        {
            var builder = new MarketReplayBuilder()
                .AddOrderBookSpread(midPrice: 5000.00, spread: 0.50, levels: 3)
                .AddTick(5000.25, volume: 50)
                .AddTick(5000.50, volume: 100);

            var events = builder.Build();
            // 3 bid levels + 3 ask levels + 2 ticks = 8 events
            AssertEqual(8, events.Count);

            var harness = new NinjaScriptTestHarness();
            var receivedMarketData = new List<MockMarketDataEventArgs>();
            var receivedDepth = new List<MockMarketDepthEventArgs>();

            harness.OnMarketData(e => receivedMarketData.Add(e));
            harness.OnMarketDepth(e => receivedDepth.Add(e));

            var player = new MarketReplayPlayer(events);
            int played = player.PlayToEnd(harness);

            AssertEqual(8, played);
            AssertEqual(6, receivedDepth.Count);
            AssertEqual(2, receivedMarketData.Count);
            AssertEqual(5000.50, receivedMarketData[1].Price);
            AssertEqual(100, receivedMarketData[1].Volume);

            // Verify Volume Profile aggregated automatically from trades
            AssertEqual(150, harness.MarketDepth.VolumeProfile.TotalVolume);
        }
    }
}
