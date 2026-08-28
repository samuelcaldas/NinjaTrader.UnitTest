using System;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Mock trading account for simulating orders, executions, positions, and balances in unit tests.
    /// </summary>
    public class MockAccount
    {
        public string Name { get; set; }
        public double CashValue { get; set; }
        public double InitialCash { get; set; }
        public List<MockOrder> Orders { get; } = new List<MockOrder>();
        public Dictionary<string, MockPosition> Positions { get; } = new Dictionary<string, MockPosition>();

        public MockAccount(string name = "SimAccount", double initialCash = 100000.0)
        {
            Name = name;
            InitialCash = initialCash;
            CashValue = initialCash;
        }

        public MockPosition GetPosition(MockInstrument instrument)
        {
            if (!Positions.TryGetValue(instrument.Name, out var position))
            {
                position = new MockPosition(instrument);
                Positions[instrument.Name] = position;
            }
            return position;
        }

        public MockOrder SubmitOrder(MockInstrument instrument, MockOrderAction action, MockOrderType orderType, int quantity, double limitPrice = 0, double stopPrice = 0, string signalName = null)
        {
            var order = new MockOrder(instrument, action, orderType, quantity, limitPrice, stopPrice, signalName)
            {
                State = MockOrderState.Submitted
            };
            Orders.Add(order);
            return order;
        }

        public void FillOrder(MockOrder order, double fillPrice, int quantity)
        {
            ValidateOrderFill(order, quantity);
            UpdateOrderFillState(order, fillPrice, quantity);

            var position = GetPosition(order.Instrument);
            int signedFillQty = order.Action == MockOrderAction.Buy ? quantity : -quantity;

            ApplyFillToPosition(position, order, fillPrice, quantity, signedFillQty);
        }

        public void CancelOrder(MockOrder order)
        {
            if (order != null && order.IsWorking)
            {
                order.State = MockOrderState.Cancelled;
            }
        }

        public double GetTotalRealizedPnL()
        {
            double total = 0;
            foreach (var pos in Positions.Values)
            {
                total += pos.RealizedPnL;
            }
            return total;
        }

        #region Private Execution Helpers

        private static void ValidateOrderFill(MockOrder order, int quantity)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (quantity <= 0)
                throw new ArgumentException("Fill quantity must be positive", nameof(quantity));
        }

        private static void UpdateOrderFillState(MockOrder order, double fillPrice, int quantity)
        {
            order.FilledQuantity += quantity;
            order.AverageFillPrice = fillPrice;
            order.State = order.FilledQuantity >= order.Quantity ? MockOrderState.Filled : MockOrderState.PartFilled;
        }

        private void ApplyFillToPosition(MockPosition pos, MockOrder order, double fillPrice, int quantity, int signedFillQty)
        {
            if (pos.IsFlat)
            {
                OpenInitialPosition(pos, fillPrice, signedFillQty);
                return;
            }

            if ((pos.IsLong && signedFillQty > 0) || (pos.IsShort && signedFillQty < 0))
            {
                IncreasePosition(pos, fillPrice, quantity, signedFillQty);
                return;
            }

            ReduceOrReversePosition(pos, order, fillPrice, quantity);
        }

        private static void OpenInitialPosition(MockPosition pos, double fillPrice, int signedFillQty)
        {
            pos.Quantity = signedFillQty;
            pos.AveragePrice = fillPrice;
        }

        private static void IncreasePosition(MockPosition pos, double fillPrice, int quantity, int signedFillQty)
        {
            double totalCost = (pos.AveragePrice * Math.Abs(pos.Quantity)) + (fillPrice * quantity);
            pos.Quantity += signedFillQty;
            pos.AveragePrice = totalCost / Math.Abs(pos.Quantity);
        }

        private void ReduceOrReversePosition(MockPosition pos, MockOrder order, double fillPrice, int quantity)
        {
            int closingQty = Math.Min(Math.Abs(pos.Quantity), quantity);
            double pnl = order.Instrument.CalculatePnL(pos.AveragePrice, fillPrice, closingQty, pos.IsLong);
            pos.RealizedPnL += pnl;
            CashValue += pnl;

            int remainingOldQty = pos.Quantity + (pos.IsLong ? -closingQty : closingQty);
            int excessQty = quantity - closingQty;

            if (excessQty > 0)
            {
                pos.Quantity = (order.Action == MockOrderAction.Buy) ? excessQty : -excessQty;
                pos.AveragePrice = fillPrice;
                return;
            }

            pos.Quantity = remainingOldQty;
            if (pos.IsFlat)
            {
                pos.AveragePrice = 0;
            }
        }

        #endregion
    }
}
