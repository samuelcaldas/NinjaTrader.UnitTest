using System;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Mock order representation for unit testing order management and execution logic.
    /// </summary>
    public class MockOrder
    {
        public string OrderId { get; set; }
        public MockInstrument Instrument { get; set; }
        public MockOrderAction Action { get; set; }
        public MockOrderType OrderType { get; set; }
        public int Quantity { get; set; }
        public double LimitPrice { get; set; }
        public double StopPrice { get; set; }
        public MockOrderState State { get; set; }
        public int FilledQuantity { get; set; }
        public double AverageFillPrice { get; set; }
        public string SignalName { get; set; }
        public string FromEntrySignal { get; set; }
        public string OcoGroupId { get; set; }
        public MockTimeInForce TimeInForce { get; set; } = MockTimeInForce.Gtc;
        public DateTime Timestamp { get; set; }
        public string ErrorCode { get; set; }
        public string CancelReason { get; set; }
        public List<MockExecution> Executions { get; } = new List<MockExecution>();

        public string Name
        {
            get => SignalName;
            set => SignalName = value;
        }

        public int Filled => FilledQuantity;

        public MockOrder(
            MockInstrument instrument,
            MockOrderAction action,
            MockOrderType orderType,
            int quantity,
            double limitPrice = 0,
            double stopPrice = 0,
            string signalName = null,
            string ocoGroupId = null)
        {
            OrderId = Guid.NewGuid().ToString("N");
            Instrument = instrument;
            Action = action;
            OrderType = orderType;
            Quantity = quantity;
            LimitPrice = limitPrice;
            StopPrice = stopPrice;
            State = MockOrderState.Initialized;
            SignalName = signalName;
            OcoGroupId = ocoGroupId;
            Timestamp = DateTime.Now;
        }

        public bool IsFilled => State == MockOrderState.Filled;
        public bool IsWorking => State == MockOrderState.Working || State == MockOrderState.Accepted || State == MockOrderState.Submitted;
        public bool IsCancelled => State == MockOrderState.Cancelled;
        public bool IsTerminalState => State == MockOrderState.Filled || State == MockOrderState.Cancelled || State == MockOrderState.Rejected;
    }
}
