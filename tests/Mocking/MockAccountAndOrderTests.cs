using System;
using NinjaTrader.UnitTest;
using NinjaTrader.UnitTest.Mocking;

namespace NinjaTrader.UnitTest.Tests.Mocking
{
    public class MockAccountAndOrderTests : TestCase
    {
        public void TestMockAccountAndOrderFills()
        {
            var account = new MockAccount("TestAccount", 50000.0);
            var nq = MockInstrument.CreateFutures("NQ", tickSize: 0.25, pointValue: 20.0);

            var buyOrder = account.SubmitOrder(nq, MockOrderAction.Buy, MockOrderType.Market, 2);
            AssertEqual(MockOrderState.Submitted, buyOrder.State);

            // Fill at 18000.00
            account.FillOrder(buyOrder, 18000.0, 2);
            AssertTrue(buyOrder.IsFilled);

            var pos = account.GetPosition(nq);
            AssertEqual(2, pos.Quantity);
            AssertEqual(18000.0, pos.AveragePrice);
            AssertTrue(pos.IsLong);

            // Unrealized PnL at 18050.00 = 50 pts * $20 * 2 = $2000
            double unrealized = pos.GetUnrealizedPnL(18050.0);
            AssertEqual(2000.0, unrealized);

            // Sell 2 contracts at 18050.00 to close
            var sellOrder = account.SubmitOrder(nq, MockOrderAction.Sell, MockOrderType.Market, 2);
            account.FillOrder(sellOrder, 18050.0, 2);

            AssertEqual(0, pos.Quantity);
            AssertTrue(pos.IsFlat);
            AssertEqual(2000.0, pos.RealizedPnL);
            AssertEqual(52000.0, account.CashValue);
        }
    }
}
