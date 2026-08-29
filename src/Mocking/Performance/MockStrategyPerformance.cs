using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Computes and aggregates strategy performance metrics from completed trades.
    /// </summary>
    public class MockStrategyPerformance
    {
        private readonly List<MockTrade> _trades = new List<MockTrade>();

        public IReadOnlyList<MockTrade> Trades => _trades;

        public int TotalTrades => _trades.Count;
        public int WinningTrades => _trades.Count(t => t.IsWinner);
        public int LosingTrades => _trades.Count(t => t.IsLoser);
        public int EvenTrades => _trades.Count(t => t.IsEven);

        public double WinRate => TotalTrades > 0 ? (double)WinningTrades / TotalTrades : 0.0;

        public double GrossProfit => _trades.Where(t => t.GrossProfit > 0).Sum(t => t.GrossProfit);
        public double GrossLoss => Math.Abs(_trades.Where(t => t.GrossProfit < 0).Sum(t => t.GrossProfit));
        public double TotalCommission => _trades.Sum(t => t.Commission);
        public double NetProfit => GrossProfit - GrossLoss - TotalCommission;

        public double ProfitFactor
        {
            get
            {
                if (GrossLoss == 0)
                    return GrossProfit > 0 ? double.PositiveInfinity : 0.0;

                return GrossProfit / GrossLoss;
            }
        }

        public double AverageTrade => TotalTrades > 0 ? NetProfit / TotalTrades : 0.0;

        public double MaxDrawdown
        {
            get
            {
                double peak = 0.0;
                double maxDd = 0.0;
                double cumulative = 0.0;

                foreach (var trade in _trades)
                {
                    cumulative += trade.NetProfit;
                    if (cumulative > peak)
                        peak = cumulative;

                    double currentDd = peak - cumulative;
                    if (currentDd > maxDd)
                        maxDd = currentDd;
                }

                return maxDd;
            }
        }

        public void AddTrade(MockTrade trade)
        {
            if (trade == null)
                throw new ArgumentNullException(nameof(trade));

            trade.TradeNumber = _trades.Count + 1;
            _trades.Add(trade);
        }

        public void Clear()
        {
            _trades.Clear();
        }
    }
}
