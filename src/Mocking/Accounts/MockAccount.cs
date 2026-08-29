using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Mock trading account for simulating orders, executions, positions, OCO brackets, and balances in unit tests.
    /// </summary>
    public class MockAccount
    {
        public string Name { get; set; }
        public double CashValue { get; set; }
        public double InitialCash { get; set; }
        public List<MockOrder> Orders { get; } = new List<MockOrder>();
        public List<MockExecution> Executions { get; } = new List<MockExecution>();
        public Dictionary<string, MockPosition> Positions { get; } = new Dictionary<string, MockPosition>();
        public MockStrategyPerformance Performance { get; } = new MockStrategyPerformance();

        // Active open trade entry state for performance tracking
        private readonly Dictionary<string, (DateTime EntryTime, double EntryPrice, string EntrySignal, MockOrderAction EntryAction, double EntryCommission)> _openTradeTrackers
            = new Dictionary<string, (DateTime, double, string, MockOrderAction, double)>();

        public MockAccount(string name = "SimAccount", double initialCash = 100000.0)
        {
            Name = name;
            InitialCash = initialCash;
            CashValue = initialCash;
        }

        public MockPosition GetPosition(MockInstrument instrument)
        {
            if (instrument == null)
                throw new ArgumentNullException(nameof(instrument));

            if (!Positions.TryGetValue(instrument.Name, out var position))
            {
                position = new MockPosition(instrument);
                Positions[instrument.Name] = position;
            }
            return position;
        }

        public MockOrder SubmitOrder(
            MockInstrument instrument,
            MockOrderAction action,
            MockOrderType orderType,
            int quantity,
            double limitPrice = 0,
            double stopPrice = 0,
            string signalName = null,
            string ocoGroupId = null,
            MockTimeInForce timeInForce = MockTimeInForce.Gtc,
            string fromEntrySignal = null)
        {
            var order = new MockOrder(instrument, action, orderType, quantity, limitPrice, stopPrice, signalName, ocoGroupId)
            {
                State = MockOrderState.Submitted,
                TimeInForce = timeInForce,
                FromEntrySignal = fromEntrySignal
            };
            Orders.Add(order);
            return order;
        }

        public (MockOrder EntryOrder, MockOrder StopLossOrder, MockOrder ProfitTargetOrder) SubmitBracket(
            MockInstrument instrument,
            MockOrderAction action,
            int quantity,
            double stopPrice,
            double targetPrice,
            string entrySignal = "Entry",
            string stopSignal = "StopLoss",
            string targetSignal = "ProfitTarget",
            MockOrderType entryType = MockOrderType.Market,
            double entryLimitPrice = 0)
        {
            string ocoGroup = $"OCO_{Guid.NewGuid():N}";

            var entryOrder = SubmitOrder(instrument, action, entryType, quantity, entryLimitPrice, 0, entrySignal);

            MockOrderAction exitAction = (action == MockOrderAction.Buy) ? MockOrderAction.Sell : MockOrderAction.Buy;

            var stopOrder = SubmitOrder(instrument, exitAction, MockOrderType.StopMarket, quantity, 0, stopPrice, stopSignal, ocoGroup, MockTimeInForce.Gtc, entrySignal);
            var targetOrder = SubmitOrder(instrument, exitAction, MockOrderType.Limit, quantity, targetPrice, 0, targetSignal, ocoGroup, MockTimeInForce.Gtc, entrySignal);

            return (entryOrder, stopOrder, targetOrder);
        }

        public void SubmitOCO(MockOrder order1, MockOrder order2)
        {
            if (order1 == null || order2 == null)
                return;

            string ocoId = order1.OcoGroupId ?? order2.OcoGroupId ?? $"OCO_{Guid.NewGuid():N}";
            order1.OcoGroupId = ocoId;
            order2.OcoGroupId = ocoId;
        }

        public MockExecution FillOrder(MockOrder order, double fillPrice, int quantity, DateTime? fillTime = null)
        {
            ValidateOrderFill(order, quantity);
            DateTime executionTime = fillTime ?? DateTime.Now;

            double commission = order.Instrument?.CalculateCommission(quantity) ?? 0.0;
            CashValue -= commission;

            var execution = new MockExecution(order, fillPrice, quantity, executionTime, commission);
            Executions.Add(execution);
            order.Executions.Add(execution);

            UpdateOrderFillState(order, fillPrice, quantity);

            var position = GetPosition(order.Instrument);
            position.TotalCommissions += commission;

            int signedFillQty = order.Action == MockOrderAction.Buy ? quantity : -quantity;
            ApplyFillToPosition(position, order, fillPrice, quantity, signedFillQty, executionTime, commission);

            // Handle OCO cancellation when order fills completely
            if (order.IsFilled && !string.IsNullOrEmpty(order.OcoGroupId))
            {
                CancelOcoGroup(order.OcoGroupId, order.OrderId);
            }

            return execution;
        }

        public void CancelOrder(MockOrder order)
        {
            if (order != null && order.IsWorking)
            {
                order.State = MockOrderState.Cancelled;
                if (!string.IsNullOrEmpty(order.OcoGroupId))
                {
                    CancelOcoGroup(order.OcoGroupId, order.OrderId);
                }
            }
        }

        public void CancelOcoGroup(string ocoGroupId, string excludeOrderId = null)
        {
            if (string.IsNullOrEmpty(ocoGroupId))
                return;

            foreach (var o in Orders.Where(o => o.OcoGroupId == ocoGroupId && o.OrderId != excludeOrderId && o.IsWorking))
            {
                o.State = MockOrderState.Cancelled;
            }
        }

        public void ProcessWorkingOrders(MockInstrument instrument, double highPrice, double lowPrice, double closePrice, DateTime? time = null)
        {
            if (instrument == null)
                return;

            var workingOrders = Orders.Where(o => o.Instrument?.Name == instrument.Name && o.IsWorking).ToList();

            foreach (var order in workingOrders)
            {
                if (!order.IsWorking)
                    continue;

                int qtyRemaining = order.Quantity - order.FilledQuantity;
                if (qtyRemaining <= 0)
                    continue;

                // 1. Limit Orders
                if (order.OrderType == MockOrderType.Limit)
                {
                    if (order.Action == MockOrderAction.Buy && lowPrice <= order.LimitPrice)
                    {
                        FillOrder(order, order.LimitPrice, qtyRemaining, time);
                    }
                    else if ((order.Action == MockOrderAction.Sell || order.Action == MockOrderAction.SellShort) && highPrice >= order.LimitPrice)
                    {
                        FillOrder(order, order.LimitPrice, qtyRemaining, time);
                    }
                }
                // 2. Stop Market Orders
                else if (order.OrderType == MockOrderType.StopMarket || order.OrderType == MockOrderType.StopLimit)
                {
                    if (order.Action == MockOrderAction.Buy && highPrice >= order.StopPrice)
                    {
                        FillOrder(order, order.StopPrice, qtyRemaining, time);
                    }
                    else if ((order.Action == MockOrderAction.Sell || order.Action == MockOrderAction.SellShort) && lowPrice <= order.StopPrice)
                    {
                        FillOrder(order, order.StopPrice, qtyRemaining, time);
                    }
                }
                // 3. Market If Touched (MIT)
                else if (order.OrderType == MockOrderType.Mit)
                {
                    if (order.Action == MockOrderAction.Buy && lowPrice <= order.LimitPrice)
                    {
                        FillOrder(order, order.LimitPrice, qtyRemaining, time);
                    }
                    else if ((order.Action == MockOrderAction.Sell || order.Action == MockOrderAction.SellShort) && highPrice >= order.LimitPrice)
                    {
                        FillOrder(order, order.LimitPrice, qtyRemaining, time);
                    }
                }
            }
        }

        public void UpdateTrailingStops(MockInstrument instrument, double currentPrice, double trailDelta)
        {
            if (instrument == null || trailDelta <= 0)
                return;

            var position = GetPosition(instrument);
            if (position.IsFlat)
                return;

            var stopOrders = Orders.Where(o => o.Instrument?.Name == instrument.Name && o.IsWorking &&
                (o.OrderType == MockOrderType.StopMarket || o.OrderType == MockOrderType.StopLimit)).ToList();

            foreach (var stopOrder in stopOrders)
            {
                if (position.IsLong)
                {
                    double targetStop = currentPrice - trailDelta;
                    if (targetStop > stopOrder.StopPrice)
                    {
                        stopOrder.StopPrice = instrument.RoundToTick(targetStop);
                    }
                }
                else if (position.IsShort)
                {
                    double targetStop = currentPrice + trailDelta;
                    if (stopOrder.StopPrice == 0 || targetStop < stopOrder.StopPrice)
                    {
                        stopOrder.StopPrice = instrument.RoundToTick(targetStop);
                    }
                }
            }
        }

        public double GetTotalRealizedPnL()
        {
            return Positions.Values.Sum(pos => pos.RealizedPnL);
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

        private void ApplyFillToPosition(MockPosition pos, MockOrder order, double fillPrice, int quantity, int signedFillQty, DateTime fillTime, double commission)
        {
            string instKey = pos.Instrument.Name;

            if (pos.IsFlat)
            {
                OpenInitialPosition(pos, fillPrice, signedFillQty);
                _openTradeTrackers[instKey] = (fillTime, fillPrice, order.SignalName, order.Action, commission);
                pos.ResetExcursions();
                return;
            }

            if ((pos.IsLong && signedFillQty > 0) || (pos.IsShort && signedFillQty < 0))
            {
                IncreasePosition(pos, fillPrice, quantity, signedFillQty);
                if (_openTradeTrackers.TryGetValue(instKey, out var existing))
                {
                    _openTradeTrackers[instKey] = (existing.EntryTime, pos.AveragePrice, existing.EntrySignal, existing.EntryAction, existing.EntryCommission + commission);
                }
                return;
            }

            ReduceOrReversePosition(pos, order, fillPrice, quantity, fillTime, commission);
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

        private void ReduceOrReversePosition(MockPosition pos, MockOrder order, double fillPrice, int quantity, DateTime fillTime, double commission)
        {
            string instKey = pos.Instrument.Name;
            int closingQty = Math.Min(Math.Abs(pos.Quantity), quantity);
            double pnl = order.Instrument.CalculatePnL(pos.AveragePrice, fillPrice, closingQty, pos.IsLong);
            pos.RealizedPnL += pnl;
            CashValue += pnl;

            // Record completed trade to performance tracker
            if (_openTradeTrackers.TryGetValue(instKey, out var entryData))
            {
                double totalTradeCommission = entryData.EntryCommission + commission;
                var trade = new MockTrade
                {
                    Instrument = pos.Instrument,
                    EntryAction = entryData.EntryAction,
                    EntryPrice = entryData.EntryPrice,
                    ExitPrice = fillPrice,
                    Quantity = closingQty,
                    EntryTime = entryData.EntryTime,
                    ExitTime = fillTime,
                    EntrySignal = entryData.EntrySignal,
                    ExitSignal = order.SignalName,
                    GrossProfit = pnl,
                    Commission = totalTradeCommission,
                    MaxAdverseExcursion = pos.MaxAdverseExcursion,
                    MaxFavorableExcursion = pos.MaxFavorableExcursion
                };
                Performance.AddTrade(trade);
            }

            int remainingOldQty = pos.Quantity + (pos.IsLong ? -closingQty : closingQty);
            int excessQty = quantity - closingQty;

            if (excessQty > 0)
            {
                pos.Quantity = (order.Action == MockOrderAction.Buy) ? excessQty : -excessQty;
                pos.AveragePrice = fillPrice;
                _openTradeTrackers[instKey] = (fillTime, fillPrice, order.SignalName, order.Action, commission);
                pos.ResetExcursions();
                return;
            }

            pos.Quantity = remainingOldQty;
            if (pos.IsFlat)
            {
                pos.AveragePrice = 0;
                _openTradeTrackers.Remove(instKey);
                pos.ResetExcursions();
            }
        }

        #endregion
    }
}
