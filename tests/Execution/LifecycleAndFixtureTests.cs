using System;
using NinjaTrader.UnitTest;

namespace NinjaTrader.UnitTest.Tests.Execution
{
    public class LifecycleAndFixtureTests : TestCase
    {
        private static int _classSetUpCount = 0;
        private static int _classTearDownCount = 0;
        private bool _setUpExecuted = false;
        private bool _cleanupExecuted = false;

        public new static void SetUpClass()
        {
            _classSetUpCount++;
        }

        public new static void TearDownClass()
        {
            _classTearDownCount++;
        }

        public override void SetUp()
        {
            _setUpExecuted = true;
        }

        public override void TearDown()
        {
            _setUpExecuted = false;
        }

        public void TestSetUpExecution()
        {
            AssertTrue(_setUpExecuted, "SetUp() was not executed before test method");
        }

        public void TestCleanupsExecution()
        {
            AddCleanup(() =>
            {
                _cleanupExecuted = true;
            });

            AssertFalse(_cleanupExecuted, "Cleanup should not execute during the test method");
        }
    }
}
