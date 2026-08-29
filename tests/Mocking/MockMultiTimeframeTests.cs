using System;
using NinjaTrader.UnitTest.Mocking;

namespace NinjaTrader.UnitTest
{
    public class MockMultiTimeframeTests : TestCase
    {
        public void TestBarsArrayAccess()
        {
            var minute1Bars = new BarSeriesBuilder("ES", timeStep: TimeSpan.FromMinutes(1))
                .AddBar(5000, 5005, 4995, 5002)
                .AddBar(5002, 5010, 5000, 5008)
                .Build();
            minute1Bars.PeriodType = MockBarsPeriodType.Minute;
            minute1Bars.PeriodValue = 1;

            var minute5Bars = new BarSeriesBuilder("ES", timeStep: TimeSpan.FromMinutes(5))
                .AddBar(5000, 5010, 4995, 5008)
                .Build();
            minute5Bars.PeriodType = MockBarsPeriodType.Minute;
            minute5Bars.PeriodValue = 5;

            var barsArray = new MockBarsArray(minute1Bars, minute5Bars);

            AssertEqual(2, barsArray.Count);
            AssertIs(minute1Bars, barsArray.Primary);
            AssertIs(minute1Bars, barsArray[0]);
            AssertIs(minute5Bars, barsArray[1]);

            AssertEqual(MockBarsPeriodType.Minute, barsArray[0].PeriodType);
            AssertEqual(1, barsArray[0].PeriodValue);
            AssertEqual(5, barsArray[1].PeriodValue);
        }

        public void TestHarnessMultiTimeframeIntegration()
        {
            var primary1Min = new BarSeriesBuilder("ES").AddTrend(5, 5000, 1.0).Build();
            var secondary5Min = new BarSeriesBuilder("NQ").AddTrend(5, 18000, 5.0).Build();

            var harness = new NinjaScriptTestHarness(primary1Min);
            harness.AddDataSeries(secondary5Min);

            AssertEqual(2, harness.BarsArray.Count);
            AssertEqual("ES", harness.BarsArray[0].InstrumentName);
            AssertEqual("NQ", harness.BarsArray[1].InstrumentName);
        }
    }
}
