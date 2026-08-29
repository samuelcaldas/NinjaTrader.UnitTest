using System;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Represents an open or flat market position in a mock account with excursion and PnL tracking.
    /// </summary>
    public class MockPosition
    {
        public MockInstrument Instrument { get; }
        public int Quantity { get; internal set; }
        public double AveragePrice { get; internal set; }
        public double RealizedPnL { get; internal set; }
        public double MaxAdverseExcursion { get; internal set; }
        public double MaxFavorableExcursion { get; internal set; }
        public double TotalCommissions { get; internal set; }

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
                return 0.0;

            return Instrument.CalculatePnL(AveragePrice, currentPrice, Math.Abs(Quantity), IsLong);
        }

        public void UpdateExcursions(double highPrice, double lowPrice)
        {
            if (IsFlat)
                return;

            double pnlAtHigh = Instrument.CalculatePnL(AveragePrice, highPrice, Math.Abs(Quantity), IsLong);
            double pnlAtLow = Instrument.CalculatePnL(AveragePrice, lowPrice, Math.Abs(Quantity), IsLong);

            double maxPnl = Math.Max(pnlAtHigh, pnlAtLow);
            double minPnl = Math.Min(pnlAtHigh, pnlAtLow);

            if (maxPnl > MaxFavorableExcursion)
                MaxFavorableExcursion = maxPnl;

            if (minPnl < MaxAdverseExcursion)
                MaxAdverseExcursion = minPnl;
        }

        public void ResetExcursions()
        {
            MaxAdverseExcursion = 0.0;
            MaxFavorableExcursion = 0.0;
        }
    }
}
