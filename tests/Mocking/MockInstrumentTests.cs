using System;
using NinjaTrader.UnitTest;
using NinjaTrader.UnitTest.Mocking;

namespace NinjaTrader.UnitTest.Tests.Mocking
{
    public class MockInstrumentTests : TestCase
    {
        public void TestMockInstrumentCalculations()
        {
            var es = MockInstrument.CreateFutures("ES", tickSize: 0.25, pointValue: 50.0);

            // Tick rounding
            AssertEqual(5000.25, es.RoundToTick(5000.20));
            AssertEqual(5000.50, es.RoundToTick(5000.40));

            // Tick distance: 1.0 pt = 4 ticks
            AssertEqual(4.0, es.CalculateTicks(1.0));

            // PnL calculation: 2 contracts long bought at 5000 and sold at 5010 = 10 pts * $50 * 2 = $1000
            double longPnL = es.CalculatePnL(5000.0, 5010.0, 2, isLong: true);
            AssertEqual(1000.0, longPnL);

            // Short PnL: 2 contracts shorted at 5010 and covered at 5000 = 10 pts * $50 * 2 = $1000
            double shortPnL = es.CalculatePnL(5010.0, 5000.0, 2, isLong: false);
            AssertEqual(1000.0, shortPnL);
        }
    }
}
