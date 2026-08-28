using System;
using System.Collections.Generic;
using NinjaTrader.UnitTest;
using NinjaTrader.UnitTest.Mocking;

namespace NinjaTrader.UnitTest.Tests.Mocking
{
    public class NinjaScriptTestHarnessTests : TestCase
    {
        public void TestNinjaScriptTestHarnessExecution()
        {
            var bars = new BarSeriesBuilder("ES")
                .AddTrend(barCount: 5, startPrice: 5000, stepPerBar: 5)
                .Build();

            var harness = new NinjaScriptTestHarness(bars);
            var visitedStates = new List<MockState>();
            var processedBars = new List<int>();

            harness.OnStateChange(state => visitedStates.Add(state));
            harness.OnBarUpdate(barIndex => processedBars.Add(barIndex));

            harness.RunAllBars();

            AssertTrue(visitedStates.Contains(MockState.SetDefaults));
            AssertTrue(visitedStates.Contains(MockState.Configure));
            AssertTrue(visitedStates.Contains(MockState.DataLoaded));
            AssertTrue(visitedStates.Contains(MockState.Historical));
            AssertEqual(5, processedBars.Count);
            AssertEqual(4, harness.CurrentBar);
        }
    }
}
