using Microsoft.Extensions.Options;
using StateR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BrowsableStatesStartupExtensions
    {
        public static IStatorBuilder AddBrowsableStates(this IStatorBuilder builder)
        {
            var iBrowsableStateType = typeof(IBrowsableState<>);
            var browsableStateDecoratorType = typeof(BrowsableStateDecorator<>);
            foreach (var type in builder.States)
            {
                // Equivalent to: 
                // - Decorate<IState<TState>, BrowsableStateDecorator<TState>>();
                // - AddSingleton<IBrowsableState<TState>>(p => p.GetRequiredService<IState<TState>>());
                var stateServiceType = StatorStartupExtensions.iStateType.MakeGenericType(type);
                var browsableStateServiceType = iBrowsableStateType.MakeGenericType(type);
                var browsableStateImplementationType = browsableStateDecoratorType.MakeGenericType(type);
                builder.Services.Decorate(stateServiceType, browsableStateImplementationType);
                builder.Services.AddSingleton(browsableStateServiceType, p => p.GetRequiredService(stateServiceType));
            }
            builder.Services
                .Configure<BrowsableStateOptions>(options => { })
                .AddSingleton(ctx => ctx.GetService<IOptionsMonitor<BrowsableStateOptions>>().CurrentValue)
            ;
            return builder;
        }


        private class BrowsableStateDecorator<TState> : IBrowsableState<TState>, IState<TState>, IDisposable
            where TState : StateBase
        {
            private readonly List<StateHistoryEntry<TState>> _states = new();
            private readonly BrowsableStateOptions _options;

            private int _cursorIndex;

            public BrowsableStateDecorator(IState<TState> state, BrowsableStateOptions options)
            {
                _state = state ?? throw new ArgumentNullException(nameof(state));
                _options = options ?? throw new ArgumentNullException(nameof(options));
                _state.Subscribe(StateChanged);
                PushCurrentState();
            }

            private void StateChanged()
            {
                if (Current == Cursor?.State)
                {
                    return;
                }
                PushCurrentState();
            }

            private void PushCurrentState() => PushState(Current);
            private void PushState(TState state)
            {
                if (state == Cursor?.State)
                {
                    return;
                }

                _states.Add(new StateHistoryEntry<TState>(state, DateTime.UtcNow));
                ResetCursorPosition();

                if (_options.HasAMaxHistoryLength() && _states.Count >= _options.MaxHistoryLength)
                {
                    _states.RemoveAt(0);
                    ResetCursorPosition();
                }

                void ResetCursorPosition()
                {
                    _cursorIndex = _states.Count - 1;
                }
            }

            private StateHistoryEntry<TState>[] _history;
            private int _lastStateCount = 0;
            public IReadOnlyCollection<StateHistoryEntry<TState>> History
            {
                get
                {
                    if (_history is null || _states.Count != _lastStateCount)
                    {
                        _history = _states.ToArray().Reverse().ToArray();
                        _lastStateCount = _states.Count;
                    }
                    return _history;
                }
            }

            public StateHistoryEntry<TState> Cursor
                => _states.Count > _cursorIndex ? _states[_cursorIndex] : default;

            public bool Redo()
            {
                if (_cursorIndex + 1 >= _states.Count)
                {
                    return false;
                }
                _cursorIndex++;
                Set(Cursor.State);
                Notify();
                return true;
            }

            public bool Undo()
            {
                if (_cursorIndex - 1 < 0)
                {
                    return false;
                }
                _cursorIndex--;
                Set(Cursor.State);
                Notify();
                return true;
            }

            #region IState<TState> Facade

            private readonly IState<TState> _state;

            public TState Current => _state.Current;

            public void Set(TState state)
            {
                _state.Set(state);
            }

            public void Subscribe(Action stateHasChangedDelegate)
            {
                _state.Subscribe(stateHasChangedDelegate);
            }

            public void Unsubscribe(Action stateHasChangedDelegate)
            {
                _state.Unsubscribe(stateHasChangedDelegate);
            }

            public void Notify()
            {
                _state.Notify();
            }

            #endregion

            #region IDisposable

            private bool disposedValue;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        Unsubscribe(StateChanged);
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            #endregion
        }
    }
}
