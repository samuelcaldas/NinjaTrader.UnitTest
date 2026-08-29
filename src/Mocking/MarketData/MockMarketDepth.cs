using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Level 2 market depth level entry.
    /// </summary>
    public class MockDepthLevel
    {
        public double Price { get; set; }
        public long Volume { get; set; }
        public string MarketMaker { get; set; }

        public MockDepthLevel(double price, long volume, string marketMaker = "")
        {
            Price = price;
            Volume = volume;
            MarketMaker = marketMaker;
        }
    }

    /// <summary>
    /// Simulates Level 2 order book depth ladders (bids and asks) with Order Flow, Imbalance, and Volume Profile analytics.
    /// </summary>
    public class MockMarketDepth
    {
        private readonly List<MockDepthLevel> _bids = new List<MockDepthLevel>();
        private readonly List<MockDepthLevel> _asks = new List<MockDepthLevel>();

        public IReadOnlyList<MockDepthLevel> Bids => _bids;
        public IReadOnlyList<MockDepthLevel> Asks => _asks;

        public double BestBid => _bids.Count > 0 ? _bids[0].Price : 0.0;
        public double BestAsk => _asks.Count > 0 ? _asks[0].Price : 0.0;
        public long BestBidVolume => _bids.Count > 0 ? _bids[0].Volume : 0;
        public long BestAskVolume => _asks.Count > 0 ? _asks[0].Volume : 0;
        public double Spread => (BestBid > 0 && BestAsk > 0) ? BestAsk - BestBid : 0.0;

        public long TotalBidVolume => _bids.Sum(b => b.Volume);
        public long TotalAskVolume => _asks.Sum(a => a.Volume);

        public MockVolumeProfile VolumeProfile { get; } = new MockVolumeProfile();

        public void ProcessDepth(MockMarketDepthEventArgs e)
        {
            if (e == null)
                return;

            ProcessDepth(e.MarketDataType, e.Operation, e.Price, e.Volume, e.Position, e.MarketMaker);
        }

        public void ProcessDepth(
            MockMarketDataType marketDataType,
            MockMarketDepthOperation operation,
            double price,
            long volume,
            int position = 0,
            string marketMaker = "")
        {
            var book = (marketDataType == MockMarketDataType.Bid) ? _bids : _asks;

            switch (operation)
            {
                case MockMarketDepthOperation.Insert:
                    if (position >= 0 && position <= book.Count)
                    {
                        book.Insert(position, new MockDepthLevel(price, volume, marketMaker));
                    }
                    else
                    {
                        book.Add(new MockDepthLevel(price, volume, marketMaker));
                    }
                    break;

                case MockMarketDepthOperation.Update:
                    if (position >= 0 && position < book.Count)
                    {
                        book[position].Price = price;
                        book[position].Volume = volume;
                        book[position].MarketMaker = marketMaker;
                    }
                    break;

                case MockMarketDepthOperation.Remove:
                    if (position >= 0 && position < book.Count)
                    {
                        book.RemoveAt(position);
                    }
                    else
                    {
                        var match = book.FirstOrDefault(l => Math.Abs(l.Price - price) < 0.000001);
                        if (match != null)
                        {
                            book.Remove(match);
                        }
                    }
                    break;
            }

            SortBooks();
        }

        public long GetCumulativeBidVolume(int levels = 5)
        {
            return _bids.Take(levels).Sum(b => b.Volume);
        }

        public long GetCumulativeAskVolume(int levels = 5)
        {
            return _asks.Take(levels).Sum(a => a.Volume);
        }

        public double GetBidAskImbalance(int levels = 5)
        {
            long bids = GetCumulativeBidVolume(levels);
            long asks = GetCumulativeAskVolume(levels);
            long total = bids + asks;

            if (total == 0)
                return 0.5; // Balanced

            return (double)bids / total;
        }

        public long GetDepthAtPrice(double price, MockMarketDataType marketDataType)
        {
            var book = (marketDataType == MockMarketDataType.Bid) ? _bids : _asks;
            var level = book.FirstOrDefault(l => Math.Abs(l.Price - price) < 0.000001);
            return level?.Volume ?? 0;
        }

        public MockDepthSnapshot TakeSnapshot(DateTime? timestamp = null)
        {
            return new MockDepthSnapshot(timestamp ?? DateTime.Now, _bids, _asks, BestBid, BestAsk);
        }

        public void RecordTrade(double price, long volume, bool isAggressiveBuy)
        {
            VolumeProfile.AddTrade(price, volume, isAggressiveBuy);
        }

        public void Clear()
        {
            _bids.Clear();
            _asks.Clear();
            VolumeProfile.Clear();
        }

        private void SortBooks()
        {
            _bids.Sort((a, b) => b.Price.CompareTo(a.Price)); // Bids descending
            _asks.Sort((a, b) => a.Price.CompareTo(b.Price)); // Asks ascending
        }
    }
}
