using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Volume distribution node at a specific price level.
    /// </summary>
    public class MockPriceVolumeNode
    {
        public double Price { get; }
        public long TotalVolume => BuyVolume + SellVolume;
        public long BuyVolume { get; internal set; }
        public long SellVolume { get; internal set; }
        public long Delta => BuyVolume - SellVolume;

        public MockPriceVolumeNode(double price, long buyVolume = 0, long sellVolume = 0)
        {
            Price = price;
            BuyVolume = buyVolume;
            SellVolume = sellVolume;
        }
    }

    /// <summary>
    /// Volume Profile & Order Flow distribution analytics calculated from market depth and trade executions.
    /// </summary>
    public class MockVolumeProfile
    {
        private readonly Dictionary<double, MockPriceVolumeNode> _nodes = new Dictionary<double, MockPriceVolumeNode>();
        public double TickSize { get; set; }

        public IReadOnlyCollection<MockPriceVolumeNode> Nodes => _nodes.Values;

        public long TotalVolume => _nodes.Values.Sum(n => n.TotalVolume);
        public long CumulativeDelta => _nodes.Values.Sum(n => n.Delta);

        public MockVolumeProfile(double tickSize = 0.25)
        {
            TickSize = tickSize > 0 ? tickSize : 0.01;
        }

        public void AddTrade(double price, long volume, bool isAggressiveBuy)
        {
            double quantizedPrice = Math.Round(price / TickSize, MidpointRounding.AwayFromZero) * TickSize;

            if (!_nodes.TryGetValue(quantizedPrice, out var node))
            {
                node = new MockPriceVolumeNode(quantizedPrice);
                _nodes[quantizedPrice] = node;
            }

            if (isAggressiveBuy)
                node.BuyVolume += volume;
            else
                node.SellVolume += volume;
        }

        public MockPriceVolumeNode GetNodeAt(double price)
        {
            double quantizedPrice = Math.Round(price / TickSize, MidpointRounding.AwayFromZero) * TickSize;
            _nodes.TryGetValue(quantizedPrice, out var node);
            return node;
        }

        /// <summary>
        /// Point of Control (POC): Price level with the highest traded volume.
        /// </summary>
        public double PointOfControl => _nodes.Count > 0 ? _nodes.Values.OrderByDescending(n => n.TotalVolume).First().Price : 0.0;

        /// <summary>
        /// Value Area (typically 70% of total volume centered around POC).
        /// </summary>
        public (double ValueAreaLow, double ValueAreaHigh) CalculateValueArea(double percentage = 0.70)
        {
            if (_nodes.Count == 0)
                return (0.0, 0.0);

            long targetVolume = (long)(TotalVolume * percentage);
            var sortedByPrice = _nodes.Values.OrderBy(n => n.Price).ToList();

            var pocNode = sortedByPrice.OrderByDescending(n => n.TotalVolume).First();
            int pocIdx = sortedByPrice.IndexOf(pocNode);

            int lowIdx = pocIdx;
            int highIdx = pocIdx;
            long currentVolume = pocNode.TotalVolume;

            while (currentVolume < targetVolume && (lowIdx > 0 || highIdx < sortedByPrice.Count - 1))
            {
                long nextLowVol = (lowIdx > 0) ? sortedByPrice[lowIdx - 1].TotalVolume : 0;
                long nextHighVol = (highIdx < sortedByPrice.Count - 1) ? sortedByPrice[highIdx + 1].TotalVolume : 0;

                if (nextHighVol >= nextLowVol && highIdx < sortedByPrice.Count - 1)
                {
                    highIdx++;
                    currentVolume += sortedByPrice[highIdx].TotalVolume;
                }
                else if (lowIdx > 0)
                {
                    lowIdx--;
                    currentVolume += sortedByPrice[lowIdx].TotalVolume;
                }
                else if (highIdx < sortedByPrice.Count - 1)
                {
                    highIdx++;
                    currentVolume += sortedByPrice[highIdx].TotalVolume;
                }
                else
                {
                    break;
                }
            }

            return (sortedByPrice[lowIdx].Price, sortedByPrice[highIdx].Price);
        }

        public void Clear()
        {
            _nodes.Clear();
        }
    }
}
