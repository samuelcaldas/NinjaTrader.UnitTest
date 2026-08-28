using System;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Mock instrument specification for unit testing market and PnL calculations.
    /// </summary>
    public class MockInstrument
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public double TickSize { get; set; }
        public double PointValue { get; set; }
        public MockInstrumentType InstrumentType { get; set; }
        public string Currency { get; set; }

        public MockInstrument(string name, double tickSize, double pointValue, MockInstrumentType instrumentType = MockInstrumentType.Stock, string currency = "USD")
        {
            Name = name;
            FullName = name;
            TickSize = tickSize > 0 ? tickSize : 0.01;
            PointValue = pointValue > 0 ? pointValue : 1.0;
            InstrumentType = instrumentType;
            Currency = currency;
        }

        public double RoundToTick(double price)
        {
            return Math.Round(price / TickSize, MidpointRounding.AwayFromZero) * TickSize;
        }

        public double CalculatePnL(double entryPrice, double exitPrice, int quantity, bool isLong = true)
        {
            double diff = isLong ? (exitPrice - entryPrice) : (entryPrice - exitPrice);
            return diff * PointValue * quantity;
        }

        public double CalculateTicks(double priceDiff)
        {
            return Math.Round(priceDiff / TickSize);
        }

        #region Factory Presets

        public static MockInstrument CreateFutures(string symbol = "ES", double tickSize = 0.25, double pointValue = 50.0)
        {
            return new MockInstrument(symbol, tickSize, pointValue, MockInstrumentType.Future);
        }

        public static MockInstrument CreateMicroFutures(string symbol = "MES", double tickSize = 0.25, double pointValue = 5.0)
        {
            return new MockInstrument(symbol, tickSize, pointValue, MockInstrumentType.Future);
        }

        public static MockInstrument CreateStock(string symbol = "AAPL", double tickSize = 0.01, double pointValue = 1.0)
        {
            return new MockInstrument(symbol, tickSize, pointValue, MockInstrumentType.Stock);
        }

        public static MockInstrument CreateForex(string symbol = "EURUSD", double tickSize = 0.0001, double pointValue = 100000.0)
        {
            return new MockInstrument(symbol, tickSize, pointValue, MockInstrumentType.Forex);
        }

        public static MockInstrument CreateCrypto(string symbol = "BTCUSD", double tickSize = 0.01, double pointValue = 1.0)
        {
            return new MockInstrument(symbol, tickSize, pointValue, MockInstrumentType.Crypto);
        }

        #endregion
    }
}
