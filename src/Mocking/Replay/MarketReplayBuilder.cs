using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Fluent builder for constructing synthetic market replay and Level 2 depth event streams.
    /// </summary>
    public class MarketReplayBuilder
    {
        private readonly List<MarketReplayEvent> _events = new List<MarketReplayEvent>();
        private DateTime _currentTime;
        private readonly MockInstrument _instrument;

        public MarketReplayBuilder(MockInstrument instrument = null, DateTime? startTime = null)
        {
            _instrument = instrument ?? MockInstrument.CreateFutures("ES");
            _currentTime = startTime ?? new DateTime(2026, 1, 1, 9, 30, 0);
        }

        public MarketReplayBuilder AddTick(double price, long volume = 1, TimeSpan? offset = null)
        {
            if (offset.HasValue)
                _currentTime += offset.Value;

            _events.Add(MarketReplayEvent.CreateTick(_currentTime, price, volume, _instrument));
            return this;
        }

        public MarketReplayBuilder AddTickReplay(double lastPrice, double bidPrice, double askPrice, long volume = 1, TimeSpan? offset = null)
        {
            if (offset.HasValue)
                _currentTime += offset.Value;

            _events.Add(MarketReplayEvent.CreateTickReplay(_currentTime, lastPrice, bidPrice, askPrice, volume, _instrument));
            return this;
        }

        public MarketReplayBuilder AddDepth(
            MockMarketDataType type,
            MockMarketDepthOperation op,
            double price,
            long volume,
            int position = 0,
            string marketMaker = "",
            TimeSpan? offset = null)
        {
            if (offset.HasValue)
                _currentTime += offset.Value;

            _events.Add(MarketReplayEvent.CreateDepth(_currentTime, type, op, price, volume, position, marketMaker, _instrument));
            return this;
        }

        public MarketReplayBuilder AddOrderBookSpread(double midPrice, double spread, int levels = 5, long baseVolume = 10, TimeSpan? offset = null)
        {
            if (offset.HasValue)
                _currentTime += offset.Value;

            double tick = _instrument.TickSize;
            double halfSpread = spread / 2.0;

            double bestBid = _instrument.RoundToTick(midPrice - halfSpread);
            double bestAsk = _instrument.RoundToTick(midPrice + halfSpread);

            for (int i = 0; i < levels; i++)
            {
                double bidPrice = bestBid - (i * tick);
                double askPrice = bestAsk + (i * tick);
                long bidVol = baseVolume + (i * 5);
                long askVol = baseVolume + (i * 5);

                _events.Add(MarketReplayEvent.CreateDepth(_currentTime, MockMarketDataType.Bid, MockMarketDepthOperation.Insert, bidPrice, bidVol, i, "", _instrument));
                _events.Add(MarketReplayEvent.CreateDepth(_currentTime, MockMarketDataType.Ask, MockMarketDepthOperation.Insert, askPrice, askVol, i, "", _instrument));
            }

            return this;
        }

        public MarketReplayBuilder AddTradeSweep(MockOrderAction action, double startPrice, double endPrice, long volumePerLevel = 20, TimeSpan? stepDelay = null)
        {
            TimeSpan delay = stepDelay ?? TimeSpan.FromMilliseconds(200);
            double tick = _instrument.TickSize;

            if (action == MockOrderAction.Buy)
            {
                for (double p = startPrice; p <= endPrice; p += tick)
                {
                    _currentTime += delay;
                    p = _instrument.RoundToTick(p);
                    _events.Add(MarketReplayEvent.CreateTick(_currentTime, p, volumePerLevel, _instrument));
                    _events.Add(MarketReplayEvent.CreateDepth(_currentTime, MockMarketDataType.Ask, MockMarketDepthOperation.Update, p, 0, 0, "", _instrument));
                }
            }
            else
            {
                for (double p = startPrice; p >= endPrice; p -= tick)
                {
                    _currentTime += delay;
                    p = _instrument.RoundToTick(p);
                    _events.Add(MarketReplayEvent.CreateTick(_currentTime, p, volumePerLevel, _instrument));
                    _events.Add(MarketReplayEvent.CreateDepth(_currentTime, MockMarketDataType.Bid, MockMarketDepthOperation.Update, p, 0, 0, "", _instrument));
                }
            }

            return this;
        }

        public List<MarketReplayEvent> Build()
        {
            return new List<MarketReplayEvent>(_events);
        }

        public string ExportToCsv(MarketReplayRecordType format = MarketReplayRecordType.Tick)
        {
            var sb = new StringBuilder();

            foreach (var e in _events)
            {
                string timestampStr = e.Time.ToString("yyyyMMdd HHmmss fffffff", CultureInfo.InvariantCulture);

                if (e.RecordType == MarketReplayRecordType.MarketDepth && e.MarketDepth != null)
                {
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0};{1};{2};{3};{4};{5};{6}",
                        timestampStr,
                        e.MarketDepth.MarketDataType,
                        e.MarketDepth.Operation,
                        e.MarketDepth.Price,
                        e.MarketDepth.Volume,
                        e.MarketDepth.Position,
                        e.MarketDepth.MarketMaker));
                }
                else if (e.RecordType == MarketReplayRecordType.TickReplay)
                {
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0};{1};{2};{3};{4}",
                        timestampStr,
                        e.Price,
                        e.BidPrice,
                        e.AskPrice,
                        e.Volume));
                }
                else
                {
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0};{1};{2}",
                        timestampStr,
                        e.Price,
                        e.Volume));
                }
            }

            return sb.ToString();
        }

        public void SaveToNrd(string filePath)
        {
            using (var writer = new NrdFileWriter(filePath, _instrument))
            {
                foreach (var e in _events)
                {
                    writer.WriteEvent(e);
                }
            }
        }

        public byte[] ExportToNrdBytes()
        {
            using (var ms = new System.IO.MemoryStream())
            {
                using (var writer = new NrdFileWriter(ms, _instrument, leaveOpen: true))
                {
                    foreach (var e in _events)
                    {
                        writer.WriteEvent(e);
                    }
                }
                return ms.ToArray();
            }
        }

        public NrdRealtimePlayer ToRealtimePlayer(double speedMultiplier = 0.0)
        {
            return new NrdRealtimePlayer(_events, speedMultiplier);
        }
    }
}
