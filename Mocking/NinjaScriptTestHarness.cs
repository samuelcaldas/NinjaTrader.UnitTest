using System;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest.Mocking
{
    public enum MockState
    {
        SetDefaults,
        Configure,
        Active,
        DataLoaded,
        Historical,
        Transition,
        Realtime,
        Terminated
    }

    /// <summary>
    /// Test harness for stepping NinjaScript indicators or strategy logic through lifecycle states and bars.
    /// </summary>
    public class NinjaScriptTestHarness
    {
        public MockBarSeries Bars { get; set; }
        public MockInstrument Instrument { get; set; }
        public MockAccount Account { get; set; }
        public MockState State { get; private set; } = MockState.SetDefaults;
        public int CurrentBar { get; private set; } = -1;

        private readonly List<Action<int>> _onBarUpdateCallbacks = new List<Action<int>>();
        private readonly List<Action<MockState>> _onStateChangeCallbacks = new List<Action<MockState>>();

        public NinjaScriptTestHarness(MockBarSeries bars = null, MockInstrument instrument = null)
        {
            Bars = bars ?? new MockBarSeries();
            Instrument = instrument ?? MockInstrument.CreateFutures("ES");
            Account = new MockAccount("SimTestAccount");
        }

        public NinjaScriptTestHarness OnStateChange(Action<MockState> callback)
        {
            if (callback != null) _onStateChangeCallbacks.Add(callback);
            return this;
        }

        public NinjaScriptTestHarness OnBarUpdate(Action<int> callback)
        {
            if (callback != null) _onBarUpdateCallbacks.Add(callback);
            return this;
        }

        public void ChangeState(MockState newState)
        {
            State = newState;
            foreach (var cb in _onStateChangeCallbacks)
            {
                cb(newState);
            }
        }

        /// <summary>
        /// Transitions through standard initialization lifecycle states (SetDefaults -> Configure -> DataLoaded -> Historical).
        /// </summary>
        public void Initialize()
        {
            ChangeState(MockState.SetDefaults);
            ChangeState(MockState.Configure);
            ChangeState(MockState.DataLoaded);
            ChangeState(MockState.Historical);
        }

        /// <summary>
        /// Advances the harness by one bar and invokes OnBarUpdate callbacks.
        /// </summary>
        public bool StepNextBar()
        {
            if (CurrentBar + 1 < Bars.Count)
            {
                CurrentBar++;
                foreach (var cb in _onBarUpdateCallbacks)
                {
                    cb(CurrentBar);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Runs all remaining bars in the MockBarSeries.
        /// </summary>
        public void RunAllBars()
        {
            if (State == MockState.SetDefaults)
            {
                Initialize();
            }

            while (StepNextBar()) { }
        }

        /// <summary>
        /// Terminates the harness and transitions state to Terminated.
        /// </summary>
        public void Terminate()
        {
            ChangeState(MockState.Terminated);
        }
    }
}
