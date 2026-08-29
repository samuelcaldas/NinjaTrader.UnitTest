using System;
using System.Collections.Generic;
using System.IO;
using NinjaTrader.UnitTest.Mocking;

namespace NinjaTrader.UnitTest
{
    public class NrdReplayTests : TestCase
    {
        private MockInstrument _es;

        public override void SetUp()
        {
            _es = MockInstrument.CreateFutures("ES", tickSize: 0.25, pointValue: 50.0);
        }

        public void TestNrdHeaderSerialization()
        {
            var header = new NrdHeader
            {
                Symbol = "ES",
                TickSize = 0.25,
                PointValue = 50.0,
                TimeUniversalFirst = new DateTime(2026, 1, 5, 14, 30, 0, DateTimeKind.Utc),
                TimeUniversalLast = new DateTime(2026, 1, 5, 21, 0, 0, DateTimeKind.Utc),
                Open = 5000.00,
                High = 5025.50,
                Low = 4995.25,
                Close = 5018.75,
                TotalVolume = 150000,
                RecordCount = 3500
            };

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    header.Serialize(writer);
                }
                bytes = ms.ToArray();
            }

            NrdHeader readHeader;
            using (var ms = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(ms))
                {
                    readHeader = NrdHeader.Deserialize(reader);
                }
            }

            AssertEqual(NrdHeader.NrdMagic, readHeader.Magic);
            AssertEqual("ES", readHeader.Symbol);
            AssertEqual(0.25, readHeader.TickSize);
            AssertEqual(50.0, readHeader.PointValue);
            AssertEqual(5000.00, readHeader.Open);
            AssertEqual(5025.50, readHeader.High);
            AssertEqual(4995.25, readHeader.Low);
            AssertEqual(5018.75, readHeader.Close);
            AssertEqual(150000, readHeader.TotalVolume);
            AssertEqual(3500, readHeader.RecordCount);
        }

        public void TestRoundTripNrdFileStream()
        {
            var builder = new MarketReplayBuilder(_es, new DateTime(2026, 1, 5, 9, 30, 0))
                .AddOrderBookSpread(midPrice: 5000.00, spread: 0.50, levels: 3)
                .AddTick(5000.25, volume: 50, offset: TimeSpan.FromSeconds(1))
                .AddTick(5000.50, volume: 100, offset: TimeSpan.FromSeconds(1));

            byte[] nrdBytes = builder.ExportToNrdBytes();
            AssertTrue(nrdBytes.Length > 0);

            using (var ms = new MemoryStream(nrdBytes))
            {
                var events = MarketReplayReader.ReadNrdStream(ms, leaveOpen: true);

                // 3 bids + 3 asks + 2 ticks = 8 events
                AssertEqual(8, events.Count);

                AssertEqual(MockMarketDataType.Bid, events[0].MarketDepth.MarketDataType);
                AssertEqual(5000.25, events[6].Price);
                AssertEqual(50, events[6].Volume);
                AssertEqual(5000.50, events[7].Price);
                AssertEqual(100, events[7].Volume);
            }
        }

        public void TestNrdRealtimePlayerPlaybackAndHarnessIntegration()
        {
            var builder = new MarketReplayBuilder(_es, new DateTime(2026, 1, 5, 9, 30, 0))
                .AddOrderBookSpread(midPrice: 5000.00, spread: 0.50, levels: 2)
                .AddTick(5000.25, volume: 20)
                .AddTick(5000.50, volume: 30);

            byte[] nrdBytes = builder.ExportToNrdBytes();

            List<MarketReplayEvent> events;
            using (var ms = new MemoryStream(nrdBytes))
            {
                events = MarketReplayReader.ReadNrdStream(ms);
            }

            var harness = new NinjaScriptTestHarness(instrument: _es);
            var ticksReceived = new List<MockMarketDataEventArgs>();
            var depthReceived = new List<MockMarketDepthEventArgs>();

            harness.OnMarketData(e => ticksReceived.Add(e));
            harness.OnMarketDepth(e => depthReceived.Add(e));

            // Instant execution (SpeedMultiplier = 0.0)
            var player = new NrdRealtimePlayer(events, speedMultiplier: 0.0);
            int processed = player.PlayToEnd(harness);

            AssertEqual(6, processed);
            AssertEqual(4, depthReceived.Count);
            AssertEqual(2, ticksReceived.Count);
            AssertEqual(50, harness.MarketDepth.VolumeProfile.TotalVolume);
        }

        public void TestRealtimeOrderFillOnNrdStream()
        {
            var builder = new MarketReplayBuilder(_es, new DateTime(2026, 1, 5, 9, 30, 0))
                .AddTick(5002.00, volume: 10, offset: TimeSpan.FromSeconds(1))
                .AddTick(5000.00, volume: 10, offset: TimeSpan.FromSeconds(1)) // Penetrates limit
                .AddTick(4998.00, volume: 10, offset: TimeSpan.FromSeconds(1));

            var events = builder.Build();
            var harness = new NinjaScriptTestHarness(instrument: _es);

            // Submit working Limit Buy at 5000.00
            var buyLimit = harness.Account.SubmitOrder(_es, MockOrderAction.Buy, MockOrderType.Limit, 1, limitPrice: 5000.00);
            AssertTrue(buyLimit.IsWorking);

            var player = new NrdRealtimePlayer(events, speedMultiplier: 0.0);

            // Step 1: Price 5002.00 -> Order still working
            player.StepNext(harness);
            AssertTrue(buyLimit.IsWorking);

            // Step 2: Price 5000.00 -> Fills automatically
            player.StepNext(harness);
            AssertTrue(buyLimit.IsFilled);
            AssertEqual(5000.00, buyLimit.AverageFillPrice);
        }
    }
}
