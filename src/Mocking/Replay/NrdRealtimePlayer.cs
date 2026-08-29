using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Real-time and accelerated playback engine for NinjaTrader market replay event streams and .nrd files.
    /// </summary>
    public class NrdRealtimePlayer
    {
        private readonly List<MarketReplayEvent> _events;
        private int _currentIndex = 0;
        private volatile bool _isPaused = false;
        private volatile bool _isStopped = false;

        public int TotalEvents => _events.Count;
        public int CurrentIndex => _currentIndex;
        public bool HasMoreEvents => _currentIndex < _events.Count;
        public bool IsPaused => _isPaused;
        public bool IsStopped => _isStopped;

        /// <summary>
        /// Speed multiplier: 0.0 = Instant (max speed for headless CI/CD), 1.0 = 1x real-time pacing, 2.0 = 2x, etc.
        /// </summary>
        public double SpeedMultiplier { get; set; } = 0.0;

        public NrdRealtimePlayer(IEnumerable<MarketReplayEvent> events, double speedMultiplier = 0.0)
        {
            _events = new List<MarketReplayEvent>(events ?? new MarketReplayEvent[0]);
            SpeedMultiplier = speedMultiplier;
        }

        public bool StepNext(NinjaScriptTestHarness harness)
        {
            if (!HasMoreEvents || _isStopped)
                return false;

            var e = _events[_currentIndex++];
            DispatchEvent(e, harness);
            return true;
        }

        public int PlayToEnd(NinjaScriptTestHarness harness)
        {
            int processed = 0;
            while (StepNext(harness))
            {
                processed++;
            }
            return processed;
        }

        public async Task<int> PlayAsync(NinjaScriptTestHarness harness, CancellationToken cancellationToken = default)
        {
            if (harness == null)
                throw new ArgumentNullException(nameof(harness));

            _isStopped = false;
            _isPaused = false;
            int processed = 0;

            DateTime? lastEventTime = null;
            var stopwatch = new Stopwatch();

            while (HasMoreEvents && !_isStopped)
            {
                cancellationToken.ThrowIfCancellationRequested();

                while (_isPaused && !_isStopped)
                {
                    await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                }

                if (_isStopped)
                    break;

                var e = _events[_currentIndex];

                if (SpeedMultiplier > 0 && lastEventTime.HasValue && e.Time > lastEventTime.Value)
                {
                    TimeSpan dataTimeDiff = e.Time - lastEventTime.Value;
                    double delayMs = dataTimeDiff.TotalMilliseconds / SpeedMultiplier;

                    if (delayMs > 0)
                    {
                        if (delayMs > 15)
                        {
                            await Task.Delay((int)delayMs, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            stopwatch.Restart();
                            while (stopwatch.ElapsedMilliseconds < delayMs)
                            {
                                Thread.SpinWait(10);
                            }
                        }
                    }
                }

                lastEventTime = e.Time;
                _currentIndex++;
                DispatchEvent(e, harness);
                processed++;
            }

            return processed;
        }

        public void Pause() => _isPaused = true;
        public void Resume() => _isPaused = false;
        public void Stop() => _isStopped = true;
        public void Reset()
        {
            _currentIndex = 0;
            _isPaused = false;
            _isStopped = false;
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

                    bool isBuy = (harness.MarketDepth.BestAsk > 0 && e.Price >= harness.MarketDepth.BestAsk);
                    harness.MarketDepth.RecordTrade(e.Price, e.Volume, isBuy);

                    // Real-time order matching against current price
                    if (harness.AutoProcessOrders && harness.Account != null && harness.Instrument != null)
                    {
                        harness.Account.ProcessWorkingOrders(harness.Instrument, e.Price, e.Price, e.Price, e.Time);
                    }
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
