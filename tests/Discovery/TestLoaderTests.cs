using System;
using NinjaTrader.UnitTest;
using NinjaTrader.UnitTest.Tests.Assertions;

namespace NinjaTrader.UnitTest.Tests.Discovery
{
    public class TestLoaderTests : TestCase
    {
        public void TestAutoDiscoveryFromClass()
        {
            var suite = TestLoader.LoadTestsFromTestCase<BasicAssertionTests>();
            AssertGreaterEqual(suite.CountTestCases(), 4, "TestLoader should discover all test methods on BasicAssertionTests");
        }

        public void TestAutoDiscoveryFromAssembly()
        {
            var suite = TestLoader.LoadTestsFromAssembly(typeof(TestLoaderTests).Assembly);
            AssertGreater(suite.CountTestCases(), 15, "TestLoader should discover tests across the entire assembly");
        }
    }
}
