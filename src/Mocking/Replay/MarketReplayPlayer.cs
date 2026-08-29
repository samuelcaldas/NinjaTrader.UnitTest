using System;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Replays historical market data and Level 2 depth events into the NinjaScript test harness.
    /// </summary>
    public class MarketReplayPlayer
    {
        private readonly List<MarketReplayEvent> _events;
        private int _currentIndex = 0;

        public int TotalEvents => _events.Count;
        public int CurrentIndex => _currentIndex;
        public bool HasMoreEvents => _currentIndex < _events.Count;

        public MarketReplayPlayer(IEnumerable<MarketReplayEvent> events)
        {
            _events = new List<MarketReplayEvent>(events ?? new MarketReplayEvent[0]);
        }

        public bool StepNext(NinjaScriptTestHarness harness)
        {
            if (!HasMoreEvents)
                return false;

            var e = _events[_currentIndex++];
            DispatchEvent(e, harness);
            return true;
        }

        public int PlayToEnd(NinjaScriptTestHarness harness)
        {
            int count = 0;
            while (StepNext(harness))
            {
                count++;
            }
            return count;
        }

        public int PlayUntil(DateTime timestamp, NinjaScriptTestHarness harness)
        {
            int count = 0;
            while (HasMoreEvents && _events[_currentIndex].Time <= timestamp)
            {
                StepNext(harness);
                count++;
            }
            return count;
        }

        public void Reset()
        {
            _currentIndex = 0;
        }

        private static void DispatchEvent(MarketReplayEvent e, NinjaScriptTestHarness harness)
        {
            if (harness == null || e == null)
                return;

            switch (e.RecordType)
            {
                case MarketReplayRecordType.MarketDepth:
                    if (e.MarketDepth != null)
                    {
                        harness.TriggerMarketDepth(
                            e.MarketDepth.MarketDataType,
                            e.MarketDepth.Operation,
                            e.MarketDepth.Price,
                            e.MarketDepth.Volume,
                            e.MarketDepth.Position,
                            e.MarketDepth.MarketMaker);
                    }
                    break;

                case MarketReplayRecordType.Tick:
                case MarketReplayRecordType.TickReplay:
                    harness.TriggerMarketData(MockMarketDataType.Last, e.Price, e.Volume);

                    // Check aggressive trade side relative to current book
                    bool isBuy = (harness.MarketDepth.BestAsk > 0 && e.Price >= harness.MarketDepth.BestAsk);
                    harness.MarketDepth.RecordTrade(e.Price, e.Volume, isBuy);
                    break;

                case MarketReplayRecordType.MinuteBar:
                case MarketReplayRecordType.DayBar:
                    if (e.Bar != null && harness.Bars != null)
                    {
                        harness.Bars.Add(e.Bar);
                        harness.StepNextBar();
                    }
                    break;
            }
        }
    }
}
