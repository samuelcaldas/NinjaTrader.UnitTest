using System;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Fluent builder for constructing MockBarSeries test datasets.
    /// </summary>
    public class BarSeriesBuilder
    {
        private readonly MockBarSeries _series;
        private DateTime _currentTime;
        private readonly TimeSpan _timeStep;

        public BarSeriesBuilder(string instrumentName = "ES", DateTime? startTime = null, TimeSpan? timeStep = null)
        {
            _series = new MockBarSeries(instrumentName);
            _currentTime = startTime ?? new DateTime(2026, 1, 1, 9, 30, 0);
            _timeStep = timeStep ?? TimeSpan.FromMinutes(1);
        }

        public BarSeriesBuilder AddBar(double open, double high, double low, double close, long volume = 100)
        {
            _series.Add(_currentTime, open, high, low, close, volume);
            _currentTime = _currentTime.Add(_timeStep);
            return this;
        }

        public BarSeriesBuilder AddBar(DateTime time, double open, double high, double low, double close, long volume = 100)
        {
            _currentTime = time;
            _series.Add(time, open, high, low, close, volume);
            _currentTime = _currentTime.Add(_timeStep);
            return this;
        }

        public BarSeriesBuilder AddBars(params (double open, double high, double low, double close)[] bars)
        {
            if (bars == null)
                return this;

            foreach (var bar in bars)
            {
                AddBar(bar.open, bar.high, bar.low, bar.close);
            }
            return this;
        }

        public BarSeriesBuilder AddTrend(int barCount, double startPrice, double stepPerBar, double barRange = 2.0)
        {
            double currentPrice = startPrice;
            for (int i = 0; i < barCount; i++)
            {
                double open = currentPrice;
                double close = currentPrice + stepPerBar;
                double high = Math.Max(open, close) + barRange / 2.0;
                double low = Math.Min(open, close) - barRange / 2.0;
                AddBar(open, high, low, close);
                currentPrice = close;
            }
            return this;
        }

        public MockBarSeries Build() => _series;
    }
}
