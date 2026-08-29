using System;
using NinjaTrader.UnitTest.Mocking;

namespace NinjaTrader.UnitTest
{
    public class MockSeriesTests : TestCase
    {
        private MockBarSeries _bars;
        private MockSeries<double> _doubleSeries;
        private MockSeries<bool> _boolSeries;
        private MockSeries<string> _stringSeries;

        public override void SetUp()
        {
            _bars = new BarSeriesBuilder("ES")
                .AddBar(5000, 5010, 4990, 5005)
                .AddBar(5005, 5015, 5000, 5010)
                .AddBar(5010, 5020, 5005, 5015)
                .Build();

            _doubleSeries = new MockSeries<double>(_bars);
            _boolSeries = new MockSeries<bool>(_bars);
            _stringSeries = new MockSeries<string>(_bars);
        }

        public void TestSeriesIndexingAndValues()
        {
            // Current bar is index 2 (most recent)
            _doubleSeries[0] = 100.5; // Most recent bar
            _doubleSeries[1] = 95.0;  // 1 bar ago
            _doubleSeries[2] = 90.0;  // 2 bars ago

            AssertEqual(100.5, _doubleSeries[0]);
            AssertEqual(95.0, _doubleSeries[1]);
            AssertEqual(90.0, _doubleSeries[2]);

            AssertTrue(_doubleSeries.IsValidDataPoint(0));
            AssertTrue(_doubleSeries.IsValidDataPoint(1));
            AssertTrue(_doubleSeries.IsValidDataPoint(2));
        }

        public void TestSetAndReset()
        {
            _doubleSeries.Set(150.0);
            AssertEqual(150.0, _doubleSeries[0]);
            AssertTrue(_doubleSeries.IsValidDataPoint(0));

            _doubleSeries.Reset(0);
            AssertEqual(0.0, _doubleSeries[0]);
            AssertFalse(_doubleSeries.IsValidDataPoint(0));
        }

        public void TestGenericTypes()
        {
            _boolSeries[0] = true;
            _boolSeries[1] = false;
            AssertTrue(_boolSeries[0]);
            AssertFalse(_boolSeries[1]);

            _stringSeries[0] = "BullishEngulfing";
            _stringSeries[1] = "Doji";
            AssertEqual("BullishEngulfing", _stringSeries[0]);
            AssertEqual("Doji", _stringSeries[1]);
        }

        public void TestStandaloneSeriesWithoutBars()
        {
            var standalone = new MockSeries<int>(initialCapacity: 5);
            AssertEqual(5, standalone.Count);

            standalone[0] = 42;
            standalone[1] = 21;

            AssertEqual(42, standalone[0]);
            AssertEqual(21, standalone[1]);
            AssertEqual(42, standalone.GetValueAt(4));
        }
    }
}
