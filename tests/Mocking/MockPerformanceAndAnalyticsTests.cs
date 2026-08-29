using System;
using NinjaTrader.UnitTest.Mocking;

namespace NinjaTrader.UnitTest
{
    public class MockPerformanceAndAnalyticsTests : TestCase
    {
        private MockInstrument _es;
        private MockAccount _account;

        public override void SetUp()
        {
            _es = MockInstrument.CreateFutures("ES", tickSize: 0.25, pointValue: 50.0);
            _es.CommissionPerContract = 2.00;
            _account = new MockAccount("PerfAccount", 100000.0);
        }

        public void TestTradeRecordingAndMetrics()
        {
            // Trade 1: Win +$500 gross ($496 net)
            var entry1 = _account.SubmitOrder(_es, MockOrderAction.Buy, MockOrderType.Market, 1, signalName: "Signal1");
            _account.FillOrder(entry1, 5000.0, 1, DateTime.Now.AddMinutes(-30));

            var exit1 = _account.SubmitOrder(_es, MockOrderAction.Sell, MockOrderType.Market, 1, signalName: "Exit1");
            _account.FillOrder(exit1, 5010.0, 1, DateTime.Now.AddMinutes(-20));

            // Trade 2: Loss -$250 gross (-$254 net)
            var entry2 = _account.SubmitOrder(_es, MockOrderAction.Buy, MockOrderType.Market, 1, signalName: "Signal2");
            _account.FillOrder(entry2, 5010.0, 1, DateTime.Now.AddMinutes(-15));

            var exit2 = _account.SubmitOrder(_es, MockOrderAction.Sell, MockOrderType.Market, 1, signalName: "Exit2");
            _account.FillOrder(exit2, 5005.0, 1, DateTime.Now.AddMinutes(-5));

            var perf = _account.Performance;

            AssertEqual(2, perf.TotalTrades);
            AssertEqual(1, perf.WinningTrades);
            AssertEqual(1, perf.LosingTrades);
            AssertEqual(0.5, perf.WinRate);

            // Gross Profit: $500, Gross Loss: $250 -> Profit Factor: 2.0
            AssertEqual(500.0, perf.GrossProfit);
            AssertEqual(250.0, perf.GrossLoss);
            AssertEqual(2.0, perf.ProfitFactor);

            // Total Commissions: 4 orders * $2 = $8
            AssertEqual(8.0, perf.TotalCommission);

            // Net Profit: $500 - $250 - $8 = $242
            AssertEqual(242.0, perf.NetProfit);
            AssertEqual(121.0, perf.AverageTrade);

            // Verify individual trade objects
            AssertEqual(5000.0, perf.Trades[0].EntryPrice);
            AssertEqual(5010.0, perf.Trades[0].ExitPrice);
            AssertEqual("Signal1", perf.Trades[0].EntrySignal);
            AssertEqual("Exit1", perf.Trades[0].ExitSignal);
            AssertTrue(perf.Trades[0].IsWinner);
        }

        public void TestMaxDrawdownCalculation()
        {
            var perf = new MockStrategyPerformance();

            // Trade 1: +$1000 -> Peak = 1000
            perf.AddTrade(new MockTrade { GrossProfit = 1000, Commission = 0 });
            // Trade 2: -$400  -> Cumulative = 600, DD = 400
            perf.AddTrade(new MockTrade { GrossProfit = -400, Commission = 0 });
            // Trade 3: -$300  -> Cumulative = 300, DD = 700
            perf.AddTrade(new MockTrade { GrossProfit = -300, Commission = 0 });
            // Trade 4: +$800  -> Cumulative = 1100, Peak = 1100, DD = 0
            perf.AddTrade(new MockTrade { GrossProfit = 800, Commission = 0 });
            // Trade 5: -$200  -> Cumulative = 900, DD = 200
            perf.AddTrade(new MockTrade { GrossProfit = -200, Commission = 0 });

            AssertEqual(700.0, perf.MaxDrawdown);
            AssertEqual(900.0, perf.NetProfit);
        }
    }
}
