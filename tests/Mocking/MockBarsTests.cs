using System;
using NinjaTrader.UnitTest;
using NinjaTrader.UnitTest.Mocking;

namespace NinjaTrader.UnitTest.Tests.Mocking
{
    public class MockBarsTests : TestCase
    {
        public void TestBarSeriesBuilderAndMockBars()
        {
            var series = new BarSeriesBuilder("ES 03-26")
                .AddBar(5000.0, 5010.0, 4995.0, 5005.0, 1000)
                .AddBar(5005.0, 5020.0, 5002.0, 5018.0, 1500)
                .AddBar(5018.0, 5025.0, 5010.0, 5022.0, 1200)
                .Build();

            AssertEqual(3, series.Count);
            AssertEqual(2, series.CurrentBar);

            // 0 barsAgo (current)
            AssertEqual(5022.0, series.Close(0));
            AssertEqual(5018.0, series.Open(0));
            AssertEqual(5025.0, series.High(0));
            AssertEqual(5010.0, series.Low(0));
            AssertEqual(1200, series.Volume(0));

            // 1 bar ago
            AssertEqual(5018.0, series.Close(1));
            AssertEqual(5005.0, series.Open(1));

            // 2 bars ago
            AssertEqual(5005.0, series.Close(2));
            AssertEqual(5000.0, series.Open(2));
        }
    }
}
