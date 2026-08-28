using System;
using NinjaTrader.UnitTest;

namespace NinjaTrader.UnitTest.Tests.Execution
{
    public class SubTestTests : TestCase
    {
        public void TestSubTestIsolation()
        {
            var numbers = new int[] { 2, 4, 6, 8 };

            foreach (var n in numbers)
            {
                SubTest($"Testing even number {n}", () =>
                {
                    AssertEqual(0, n % 2);
                });
            }
        }
    }
}
