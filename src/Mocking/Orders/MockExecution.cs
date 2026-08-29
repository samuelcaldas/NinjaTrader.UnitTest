using System;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Represents an execution/fill event matching NinjaTrader's Execution object.
    /// </summary>
    public class MockExecution
    {
        public string ExecutionId { get; set; }
        public string OrderId { get; set; }
        public MockOrder Order { get; set; }
        public MockInstrument Instrument { get; set; }
        public DateTime Time { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double Commission { get; set; }
        public double Slippage { get; set; }
        public MockOrderAction Action { get; set; }

        public MockExecution(
            MockOrder order,
            double price,
            int quantity,
            DateTime? time = null,
            double commission = 0.0,
            double slippage = 0.0,
            string executionId = null)
        {
            Order = order;
            OrderId = order?.OrderId ?? Guid.NewGuid().ToString();
            Instrument = order?.Instrument;
            Action = order?.Action ?? MockOrderAction.Buy;
            Price = price;
            Quantity = quantity;
            Time = time ?? DateTime.Now;
            Commission = commission;
            Slippage = slippage;
            ExecutionId = executionId ?? Guid.NewGuid().ToString();
        }
    }
}
