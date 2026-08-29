using System;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Mock instrument specification for unit testing market, commission, slippage, and PnL calculations.
    /// </summary>
    public class MockInstrument
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public double TickSize { get; set; }
        public double PointValue { get; set; }
        public MockInstrumentType InstrumentType { get; set; }
        public string Currency { get; set; }
        public double CommissionPerContract { get; set; }
        public double CommissionPerShare { get; set; }
        public double FlatFeePerTrade { get; set; }
        public int DefaultSlippageTicks { get; set; }

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

        public double CalculateCommission(int quantity)
        {
            if (quantity <= 0)
                return 0.0;

            double commission = FlatFeePerTrade;
            if (CommissionPerContract > 0)
                commission += CommissionPerContract * quantity;
            else if (CommissionPerShare > 0)
                commission += CommissionPerShare * quantity;

            return commission;
        }

        public double ApplySlippage(double price, MockOrderAction action, int slippageTicks = -1)
        {
            int ticks = slippageTicks >= 0 ? slippageTicks : DefaultSlippageTicks;
            if (ticks == 0)
                return price;

            double slippageAmount = ticks * TickSize;
            return action == MockOrderAction.Buy ? price + slippageAmount : price - slippageAmount;
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

        public static MockInstrument CreateNasdaq(string symbol = "NQ", double tickSize = 0.25, double pointValue = 20.0)
        {
            return new MockInstrument(symbol, tickSize, pointValue, MockInstrumentType.Future);
        }

        public static MockInstrument CreateMicroNasdaq(string symbol = "MNQ", double tickSize = 0.25, double pointValue = 2.0)
        {
            return new MockInstrument(symbol, tickSize, pointValue, MockInstrumentType.Future);
        }

        public static MockInstrument CreateRussell(string symbol = "RTY", double tickSize = 0.10, double pointValue = 50.0)
        {
            return new MockInstrument(symbol, tickSize, pointValue, MockInstrumentType.Future);
        }

        public static MockInstrument CreateMicroRussell(string symbol = "M2K", double tickSize = 0.10, double pointValue = 5.0)
        {
            return new MockInstrument(symbol, tickSize, pointValue, MockInstrumentType.Future);
        }

        public static MockInstrument CreateCrudeOil(string symbol = "CL", double tickSize = 0.01, double pointValue = 1000.0)
        {
            return new MockInstrument(symbol, tickSize, pointValue, MockInstrumentType.Future);
        }

        public static MockInstrument CreateGold(string symbol = "GC", double tickSize = 0.10, double pointValue = 100.0)
        {
            return new MockInstrument(symbol, tickSize, pointValue, MockInstrumentType.Future);
        }

        public static MockInstrument CreateTreasuryBond(string symbol = "ZB", double tickSize = 0.03125, double pointValue = 1000.0)
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
