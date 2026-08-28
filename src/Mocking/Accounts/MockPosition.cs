using System;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Represents an open or flat market position in a mock account.
    /// </summary>
    public class MockPosition
    {
        public MockInstrument Instrument { get; }
        public int Quantity { get; internal set; }
        public double AveragePrice { get; internal set; }
        public double RealizedPnL { get; internal set; }

        public bool IsLong => Quantity > 0;
        public bool IsShort => Quantity < 0;
        public bool IsFlat => Quantity == 0;

        public MockPosition(MockInstrument instrument)
        {
            Instrument = instrument ?? throw new ArgumentNullException(nameof(instrument));
        }

        public double GetUnrealizedPnL(double currentPrice)
        {
            if (IsFlat)
                return 0;

            return Instrument.CalculatePnL(AveragePrice, currentPrice, Math.Abs(Quantity), IsLong);
        }
    }
}
