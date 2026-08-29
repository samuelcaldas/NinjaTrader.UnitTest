using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Test harness for stepping NinjaScript indicators or strategy logic through lifecycle states, multi-bars, order matching, and events.
    /// </summary>
    public class NinjaScriptTestHarness
    {
        public MockBarsArray BarsArray { get; } = new MockBarsArray();
        public MockBarSeries Bars
        {
            get => BarsArray.Primary;
            set
            {
                if (value != null)
                {
                    if (BarsArray.Count == 0)
                        BarsArray.Add(value);
                }
            }
        }

        public MockInstrument Instrument { get; set; }
        public MockAccount Account { get; set; }
        public MockMarketDepth MarketDepth { get; } = new MockMarketDepth();
        public MockStrategyPerformance Performance => Account?.Performance;
        public MockState State { get; private set; } = MockState.SetDefaults;
        public int CurrentBar { get; private set; } = -1;
        public int BarsInProgress { get; set; } = 0;
        public int RealtimeTransitionBarIndex { get; set; } = -1;
        public bool AutoProcessOrders { get; set; } = true;

        private readonly List<Action<int>> _onBarUpdateCallbacks = new List<Action<int>>();
        private readonly List<Action<MockState>> _onStateChangeCallbacks = new List<Action<MockState>>();
        private readonly List<Action<MockOrder>> _onOrderUpdateCallbacks = new List<Action<MockOrder>>();
        private readonly List<Action<MockExecution>> _onExecutionUpdateCallbacks = new List<Action<MockExecution>>();
        private readonly List<Action<MockPosition>> _onPositionUpdateCallbacks = new List<Action<MockPosition>>();
        private readonly List<Action<MockMarketDataEventArgs>> _onMarketDataCallbacks = new List<Action<MockMarketDataEventArgs>>();
        private readonly List<Action<MockMarketDepthEventArgs>> _onMarketDepthCallbacks = new List<Action<MockMarketDepthEventArgs>>();
        private readonly List<Action<MockConnectionStatusEventArgs>> _onConnectionStatusCallbacks = new List<Action<MockConnectionStatusEventArgs>>();

        public NinjaScriptTestHarness(MockBarSeries bars = null, MockInstrument instrument = null)
        {
            var primaryBars = bars ?? new MockBarSeries();
            BarsArray.Add(primaryBars);
            Instrument = instrument ?? MockInstrument.CreateFutures("ES");
            Account = new MockAccount("SimTestAccount");
        }

        public NinjaScriptTestHarness AddDataSeries(MockBarSeries secondarySeries)
        {
            if (secondarySeries != null)
            {
                BarsArray.Add(secondarySeries);
            }
            return this;
        }

        #region Callback Registration

        public NinjaScriptTestHarness OnStateChange(Action<MockState> callback)
        {
            if (callback != null)
                _onStateChangeCallbacks.Add(callback);

            return this;
        }

        public NinjaScriptTestHarness OnBarUpdate(Action<int> callback)
        {
            if (callback != null)
                _onBarUpdateCallbacks.Add(callback);

            return this;
        }

        public NinjaScriptTestHarness OnOrderUpdate(Action<MockOrder> callback)
        {
            if (callback != null)
                _onOrderUpdateCallbacks.Add(callback);

            return this;
        }

        public NinjaScriptTestHarness OnExecutionUpdate(Action<MockExecution> callback)
        {
            if (callback != null)
                _onExecutionUpdateCallbacks.Add(callback);

            return this;
        }

        public NinjaScriptTestHarness OnPositionUpdate(Action<MockPosition> callback)
        {
            if (callback != null)
                _onPositionUpdateCallbacks.Add(callback);

            return this;
        }

        public NinjaScriptTestHarness OnMarketData(Action<MockMarketDataEventArgs> callback)
        {
            if (callback != null)
                _onMarketDataCallbacks.Add(callback);

            return this;
        }

        public NinjaScriptTestHarness OnMarketDepth(Action<MockMarketDepthEventArgs> callback)
        {
            if (callback != null)
                _onMarketDepthCallbacks.Add(callback);

            return this;
        }

        public NinjaScriptTestHarness OnConnectionStatusUpdate(Action<MockConnectionStatusEventArgs> callback)
        {
            if (callback != null)
                _onConnectionStatusCallbacks.Add(callback);

            return this;
        }

        #endregion

        public void ChangeState(MockState newState)
        {
            State = newState;
            foreach (var cb in _onStateChangeCallbacks)
            {
                cb(newState);
            }
        }

        public void Initialize()
        {
            ChangeState(MockState.SetDefaults);
            ChangeState(MockState.Configure);
            ChangeState(MockState.DataLoaded);
            ChangeState(MockState.Historical);
        }

        public bool StepNextBar()
        {
            if (Bars == null || CurrentBar + 1 >= Bars.Count)
                return false;

            CurrentBar++;
            var bar = Bars.GetBarAt(CurrentBar);

            // Update position excursion tracking
            var position = Account?.GetPosition(Instrument);
            position?.UpdateExcursions(bar.High, bar.Low);

            // Auto-process working Limit/Stop orders against bar High/Low boundaries
            if (AutoProcessOrders && Account != null)
            {
                int priorExecCount = Account.Executions.Count;
                Account.ProcessWorkingOrders(Instrument, bar.High, bar.Low, bar.Close, bar.Time);

                // Fire order and execution event updates if new executions occurred
                if (Account.Executions.Count > priorExecCount)
                {
                    for (int i = priorExecCount; i < Account.Executions.Count; i++)
                    {
                        var exec = Account.Executions[i];
                        DispatchExecutionUpdates(exec);
                    }
                }
            }

            // Real-time state transition trigger if configured
            if (RealtimeTransitionBarIndex >= 0 && CurrentBar >= RealtimeTransitionBarIndex && State == MockState.Historical)
            {
                ChangeState(MockState.Realtime);
            }

            // Execute OnBarUpdate callbacks
            foreach (var cb in _onBarUpdateCallbacks)
            {
                cb(CurrentBar);
            }

            return true;
        }

        public void RunAllBars()
        {
            if (State == MockState.SetDefaults)
            {
                Initialize();
            }

            while (StepNextBar()) { }
        }

        public void TransitionToRealtime()
        {
            if (State != MockState.Realtime)
            {
                ChangeState(MockState.Realtime);
            }
        }

        public void Terminate()
        {
            ChangeState(MockState.Terminated);
        }

        #region Event Dispatchers

        public void TriggerMarketData(MockMarketDataType type, double price, long volume)
        {
            var args = new MockMarketDataEventArgs(type, price, volume, DateTime.Now, Instrument);
            foreach (var cb in _onMarketDataCallbacks)
            {
                cb(args);
            }
        }

        public void TriggerMarketDepth(MockMarketDataType type, MockMarketDepthOperation operation, double price, long volume, int position = 0, string marketMaker = "")
        {
            MarketDepth.ProcessDepth(type, operation, price, volume, position, marketMaker);

            var args = new MockMarketDepthEventArgs(type, operation, price, volume, position, marketMaker, DateTime.Now, Instrument);
            foreach (var cb in _onMarketDepthCallbacks)
            {
                cb(args);
            }
        }

        public void TriggerConnectionStatus(MockConnectionStatus status, string errorMessage = null)
        {
            var args = new MockConnectionStatusEventArgs(status, MockConnectionStatus.Disconnected, null, errorMessage);
            foreach (var cb in _onConnectionStatusCallbacks)
            {
                cb(args);
            }
        }

        private void DispatchExecutionUpdates(MockExecution exec)
        {
            if (exec?.Order != null)
            {
                foreach (var cb in _onOrderUpdateCallbacks)
                {
                    cb(exec.Order);
                }
            }

            foreach (var cb in _onExecutionUpdateCallbacks)
            {
                cb(exec);
            }

            var pos = Account?.GetPosition(exec.Instrument);
            if (pos != null)
            {
                foreach (var cb in _onPositionUpdateCallbacks)
                {
                    cb(pos);
                }
            }
        }

        #endregion
    }
}
