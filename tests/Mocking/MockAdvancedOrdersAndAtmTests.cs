using System;
using System.Collections.Generic;
using NinjaTrader.UnitTest.Mocking;

namespace NinjaTrader.UnitTest
{
    public class MockAdvancedOrdersAndAtmTests : TestCase
    {
        private MockInstrument _es;
        private MockAccount _account;

        public override void SetUp()
        {
            _es = MockInstrument.CreateFutures("ES", tickSize: 0.25, pointValue: 50.0);
            _es.CommissionPerContract = 2.00;
            _account = new MockAccount("AtmAccount", 100000.0);
        }

        public void TestBracketOrderAndOcoCancellation()
        {
            // Submit Bracket: Buy 2 contracts at Market, Stop at 4990, Target at 5020
            var bracket = _account.SubmitBracket(
                instrument: _es,
                action: MockOrderAction.Buy,
                quantity: 2,
                stopPrice: 4990.0,
                targetPrice: 5020.0,
                entrySignal: "LongEntry",
                stopSignal: "StopLoss",
                targetSignal: "ProfitTarget"
            );

            AssertEqual("LongEntry", bracket.EntryOrder.SignalName);
            AssertEqual("StopLoss", bracket.StopLossOrder.SignalName);
            AssertEqual("ProfitTarget", bracket.ProfitTargetOrder.SignalName);

            // Fill Entry Order at 5000.0
            _account.FillOrder(bracket.EntryOrder, 5000.0, 2);
            AssertTrue(bracket.EntryOrder.IsFilled);

            var position = _account.GetPosition(_es);
            AssertEqual(2, position.Quantity);
            AssertEqual(5000.0, position.AveragePrice);

            // When Profit Target fills, Stop Loss should automatically cancel (OCO)
            _account.FillOrder(bracket.ProfitTargetOrder, 5020.0, 2);

            AssertTrue(bracket.ProfitTargetOrder.IsFilled);
            AssertTrue(bracket.StopLossOrder.IsCancelled);
            AssertTrue(position.IsFlat);

            // Gross PnL: (5020 - 5000) * 50 * 2 = 2000.0
            // Commissions: 2 contracts in ($4) + 2 contracts out ($4) = $8
            // Net realized PnL: $2000 - $8 = $1992
            AssertEqual(2000.0, position.RealizedPnL);
            AssertEqual(8.0, position.TotalCommissions);
            AssertEqual(101992.0, _account.CashValue);
        }

        public void TestAutomatedOrderProcessingOnBars()
        {
            // Submit working Limit Buy order at 5000.0
            var buyLimit = _account.SubmitOrder(_es, MockOrderAction.Buy, MockOrderType.Limit, 1, limitPrice: 5000.0);
            AssertTrue(buyLimit.IsWorking);

            // Bar 1: Low is 5002.0 (does not reach limit price) -> Order stays working
            _account.ProcessWorkingOrders(_es, highPrice: 5010.0, lowPrice: 5002.0, closePrice: 5005.0);
            AssertTrue(buyLimit.IsWorking);

            // Bar 2: Low is 4998.0 (penetrates limit price 5000.0) -> Fills automatically
            _account.ProcessWorkingOrders(_es, highPrice: 5005.0, lowPrice: 4998.0, closePrice: 5002.0);
            AssertTrue(buyLimit.IsFilled);
            AssertEqual(5000.0, buyLimit.AverageFillPrice);
        }

        public void TestTrailingStopUpdates()
        {
            // Buy entry
            var entry = _account.SubmitOrder(_es, MockOrderAction.Buy, MockOrderType.Market, 1);
            _account.FillOrder(entry, 5000.0, 1);

            // Stop order at 4990.0
            var stop = _account.SubmitOrder(_es, MockOrderAction.Sell, MockOrderType.StopMarket, 1, stopPrice: 4990.0);

            // Price advances to 5010.0 with 5-point trailing delta (target stop: 5005.0)
            _account.UpdateTrailingStops(_es, currentPrice: 5010.0, trailDelta: 5.0);
            AssertEqual(5005.0, stop.StopPrice);

            // Price advances further to 5020.0 (target stop: 5015.0)
            _account.UpdateTrailingStops(_es, currentPrice: 5020.0, trailDelta: 5.0);
            AssertEqual(5015.0, stop.StopPrice);

            // Price pulls back to 5018.0 -> Stop should not move down (ratchet behavior)
            _account.UpdateTrailingStops(_es, currentPrice: 5018.0, trailDelta: 5.0);
            AssertEqual(5015.0, stop.StopPrice);
        }

        public void TestSlippageModel()
        {
            _es.DefaultSlippageTicks = 2; // 2 ticks = 0.50
            double buyPrice = _es.ApplySlippage(5000.0, MockOrderAction.Buy);
            double sellPrice = _es.ApplySlippage(5000.0, MockOrderAction.Sell);

            AssertEqual(5000.50, buyPrice);
            AssertEqual(4999.50, sellPrice);
        }
    }
}
