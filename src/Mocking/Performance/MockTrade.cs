using System;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Represents a completed round-trip trade for strategy performance analytics.
    /// </summary>
    public class MockTrade
    {
        public int TradeNumber { get; set; }
        public MockInstrument Instrument { get; set; }
        public MockOrderAction EntryAction { get; set; }
        public double EntryPrice { get; set; }
        public double ExitPrice { get; set; }
        public int Quantity { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime ExitTime { get; set; }
        public string EntrySignal { get; set; }
        public string ExitSignal { get; set; }
        public double GrossProfit { get; set; }
        public double Commission { get; set; }
        public double NetProfit => GrossProfit - Commission;
        public double MaxAdverseExcursion { get; set; }
        public double MaxFavorableExcursion { get; set; }

        public bool IsWinner => NetProfit > 0;
        public bool IsLoser => NetProfit < 0;
        public bool IsEven => NetProfit == 0;
        public bool IsLong => EntryAction == MockOrderAction.Buy;
        public bool IsShort => EntryAction == MockOrderAction.SellShort || EntryAction == MockOrderAction.Sell;
    }
}
