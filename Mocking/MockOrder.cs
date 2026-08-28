using System;

namespace NinjaTrader.UnitTest.Mocking
{
    public enum MockOrderAction
    {
        Buy,
        Sell
    }

    public enum MockOrderType
    {
        Market,
        Limit,
        StopMarket,
        StopLimit
    }

    public enum MockOrderState
    {
        Initialized,
        Submitted,
        Accepted,
        Working,
        PartFilled,
        Filled,
        Cancelled,
        Rejected
    }

    /// <summary>
    /// Mock order representation for unit testing order management logic.
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
        public DateTime Timestamp { get; set; }

        public MockOrder(MockInstrument instrument, MockOrderAction action, MockOrderType orderType, int quantity, double limitPrice = 0, double stopPrice = 0, string signalName = null)
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
            Timestamp = DateTime.Now;
        }

        public bool IsFilled => State == MockOrderState.Filled;
        public bool IsWorking => State == MockOrderState.Working || State == MockOrderState.Accepted || State == MockOrderState.Submitted;
        public bool IsCancelled => State == MockOrderState.Cancelled;
    }
}
