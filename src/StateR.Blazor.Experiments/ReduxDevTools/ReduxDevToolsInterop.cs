using StateR.Internal;
using Microsoft.JSInterop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StateR.Updater;
using System.Threading;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using StateR.Updater.Hooks;

namespace StateR.Blazor.ReduxDevTools
{
    public class DevToolsStateCollection : IEnumerable<Type>
    {
        private readonly IEnumerable<Type> _states;

        public DevToolsStateCollection(IEnumerable<Type> states)
        {
            _states = states ?? throw new ArgumentNullException(nameof(states));
        }

        public IEnumerator<Type> GetEnumerator()
        {
            return _states.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_states).GetEnumerator();
        }
    }

    public class ReduxDevToolsInteropInitializer
    {
        public IJSRuntime JSRuntime { get; }
        public IStore Store { get; }
        public DevToolsStateCollection States { get; }

        public ReduxDevToolsInteropInitializer(
            IJSRuntime jsRuntime,
            DevToolsStateCollection states,
            IStore store
        )
        {
            JSRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            States = states ?? throw new ArgumentNullException(nameof(states));
            Store = store ?? throw new ArgumentNullException(nameof(store));
        }
    }

    public class ReduxDevToolsInterop : IDisposable, IBeforeUpdateHook, IAfterUpdateHook
    {
        public bool DevToolsBrowserPluginDetected { get; private set; }
        private readonly IJSRuntime _jsRuntime;
        private readonly DotNetObjectReference<ReduxDevToolsInterop> _dotNetRef;
        private readonly IStore _store;

        private readonly DevToolsStateCollection _states;

        private int _historyIndex;
        private int HistoryIndex
        {
            get => _historyIndex;
            set
            {
                Console.WriteLine($"SET HistoryIndex = {value}");
                _historyIndex = value;
            }
        }

        public ReduxDevToolsInterop(ReduxDevToolsInteropInitializer initializer)
        {
            if (initializer == null) { throw new ArgumentNullException(nameof(initializer)); }

            _jsRuntime = initializer.JSRuntime;
            _states = initializer.States;
            _store = initializer.Store;
            _dotNetRef = DotNetObjectReference.Create(this);
        }

        public async ValueTask InitializeAsync()
        {
            var states = GetAppState();
            foreach (var state in _states)
            {
                _store.SubscribeToState(state, StateHasChanged);
            }
            _history.Add(new TypeState(revertStateAction, executeAction)
            {
                Status = TypeStateStatus.AfterUpdater,
            });
            await _jsRuntime.InvokeAsync<object>(
                "__StateRDevTools__.init",
                CancellationToken.None,
                _dotNetRef,
                states
            );
            void revertStateAction() => Console.WriteLine("@@INIT (revertStateAction) | TODO: implement this");
            void executeAction() => Console.WriteLine("@@INIT (executeAction) | TODO: implement this");
        }

        private void StateHasChanged()
        {
            if (!DevToolsBrowserPluginDetected)
            {
                return;
            }

        }

        [JSInvokable("DevToolsCallback")]
        public Task DevToolsCallbackAsync(string messageAsJson)
        {
            if (string.IsNullOrWhiteSpace(messageAsJson))
            {
                return Task.CompletedTask;
            }

            var message = JsonSerializer.Deserialize<BaseCallbackObject>(messageAsJson);
            switch (message?.payload?.type)
            {
                case "detected":
                    DevToolsBrowserPluginDetected = true;
                    break;

                case "COMMIT":
                    Console.WriteLine($"COMMIT | {messageAsJson}");
                    break;

                case "JUMP_TO_STATE":
                    var jumpToState = JsonSerializer.Deserialize<JumpToStateCallback>(messageAsJson);
                    if (HistoryIndex > jumpToState.payload.actionId)
                    {
                        HistoryIndex = jumpToState.payload.actionId;
                        _history[HistoryIndex].Undo();
                    }
                    else
                    {
                        HistoryIndex = jumpToState.payload.actionId;
                        _history[HistoryIndex].Redo();
                    }
                    break;
                case "JUMP_TO_ACTION":
                    var jumpToAction = JsonSerializer.Deserialize<JumpToStateCallback>(messageAsJson);
                    Console.WriteLine($"JUMP_TO_ACTION | _historyIndex: {HistoryIndex} | actionId: {jumpToAction.payload.actionId}");
                    if (HistoryIndex > jumpToAction.payload.actionId)
                    {
                        for (var i = HistoryIndex; i >= jumpToAction.payload.actionId; i--)
                        {
                            Console.WriteLine($"JUMP_TO_ACTION | RevertState {i}");
                            _history[i].Undo();
                        }
                    }
                    else
                    {
                        for (var i = HistoryIndex + 1; i <= jumpToAction.payload.actionId; i++)
                        {
                            Console.WriteLine($"JUMP_TO_ACTION | RevertState {i}");
                            _history[i].Redo();
                        }
                    }
                    HistoryIndex = jumpToAction.payload.actionId;
                    break;
            }
            return Task.CompletedTask;
        }

        void IDisposable.Dispose()
        {
            _dotNetRef.Dispose();
        }

        private Dictionary<string, object> GetAppState()
        {
            var states = new Dictionary<string, object>();
            foreach (var s in _states)
            {
                var value = _store.GetStateValue(s);
                var name = s.GetStateName();
                states.Add(name, value);
            }

            return states;
        }

        private class TypeState {
            private Action _undoStateAction;
            private Action _redoStateAction;
            public TypeState(Action undoStateAction, Action redoStateAction)
            {
                _undoStateAction = undoStateAction ?? throw new ArgumentNullException(nameof(undoStateAction));
                _redoStateAction = redoStateAction ?? throw new ArgumentNullException(nameof(redoStateAction));
            }
            public object ContextRef { get; init; }
            public TypeStateStatus Status { get; set; }

            public void Undo()
            {
                if (Status == TypeStateStatus.AfterUpdater)
                {
                    _undoStateAction();
                }
            }
            public void Redo()
            {
                if (Status == TypeStateStatus.AfterUpdater)
                {
                    _redoStateAction();
                }
            }
        }

        private enum TypeStateStatus
        {
            Unknown,
            BeforeUpdater,
            AfterUpdater
        }

        private List<TypeState> _history { get; } = new();

        public Task BeforeUpdateAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IUpdater<TAction, TState> updater, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase
        {
            var action = context.Action;
            var current = state.Current;
            var next = updater.Update(action, current);
            _history.Add(new TypeState(undoStateAction, redoStateAction)
            {
                ContextRef = updater,
                Status = TypeStateStatus.BeforeUpdater,
            });
            HistoryIndex = _history.Count - 1;
            return Task.CompletedTask;

            void undoStateAction()
            {
                Console.WriteLine($"Undo {current.GetType().GetStatorName()} to {current} from action: {typeof(TAction).GetStatorName()} with updater {updater.GetType().GetStatorName()}");
                state.Set(current);
                state.Notify();
            }

            void redoStateAction()
            {
                Console.WriteLine($"Redo {current.GetType().GetStatorName()} to {next} from action: {typeof(TAction).GetStatorName()} with updater {updater.GetType().GetStatorName()}");
                state.Set(next);
                state.Notify();
            }
        }

        public async Task AfterUpdateAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IUpdater<TAction, TState> updater, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase
        {
            var serializedActionInfo = JsonSerializer.Serialize(new ActionInfo(context.Action));
            var states = GetAppState();
            await _jsRuntime.InvokeAsync<object>(
                "__StateRDevTools__.dispatch",
                CancellationToken.None,
                serializedActionInfo,
                states
            );
            _history.LastOrDefault(h => h.ContextRef == updater).Status = TypeStateStatus.AfterUpdater;
        }
    }

    public class BaseCallbackObject<TPayload>
        where TPayload : BasePayload
    {
        public string type { get; set; }
        public TPayload payload { get; set; }
    }

    public class BaseCallbackObject : BaseCallbackObject<BasePayload> { }

    public class BasePayload
    {
        public string type { get; set; }
    }

    public class JumpToStateCallback : BaseCallbackObject<JumpToStatePayload>
    {
        public string state { get; set; }
    }
    public class JumpToStatePayload : BasePayload
    {
        public int index { get; set; }
        public int actionId { get; set; }
    }
    public class ActionInfo
    {
        public string type { get; }
        public object payload { get; }

        public ActionInfo(object action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            type = action.GetType().GetStatorName();
            payload = action;
        }
    }

    public static class StoreExtensions
    {
        public static void SubscribeToState(this IStore store, Type state, Action stateHasChangedDelegate)
        {
            var genericMethod = GetStoreMethod(store, state, nameof(IStore.Subscribe));
            genericMethod.Invoke(store, new[] { stateHasChangedDelegate });
        }

        public static void UnsubscribeToState(this IStore store, Type state, Action stateHasChangedDelegate)
        {
            var genericMethod = GetStoreMethod(store, state, nameof(IStore.Unsubscribe));
            genericMethod.Invoke(store, new[] { stateHasChangedDelegate });
        }

        public static object GetStateValue(this IStore store, Type state)
        {
            var genericMethod = GetStoreMethod(store, state, nameof(IStore.GetState));
            var result = genericMethod.Invoke(store, null);
            return result;
        }

        private static MethodInfo GetStoreMethod(IStore store, Type state, string methodName)
        {
            var iStateType = typeof(IState<>);
            if (!state.IsGenericType)
            {
                throw new InvalidStateTypeException(state);
            }
            if (!state.GetGenericTypeDefinition().IsAssignableTo(iStateType))
            {
                throw new InvalidStateTypeException(state);
            }
            var getState = store.GetType().GetMethod(methodName);
            var stateArgs = state.GetGenericArguments()[0];
            var genericMethod = getState.MakeGenericMethod(stateArgs);
            return genericMethod;
        }

        public static string GetStateName(this Type state)
        {
            var iStateType = typeof(IState<>);
            if (!state.IsGenericType)
            {
                throw new InvalidStateTypeException(state);
            }
            if (!state.GetGenericTypeDefinition().IsAssignableTo(iStateType))
            {
                throw new InvalidStateTypeException(state);
            }
            var stateArgs = state.GetGenericArguments()[0];
            //return stateArgs.GetStatorName();
            //var featuresIndex = stateArgs.FullName.IndexOf("Features.");
            //if(featuresIndex > -1)
            //{
            //    var dotIndex = stateArgs.FullName.IndexOf('.', featuresIndex);
            //    return stateArgs.FullName.Substring(dotIndex + 1);
            //}
            return stateArgs.FullName;
        }
    }

    public class InvalidStateTypeException : Exception
    {
        public InvalidStateTypeException(Type stateType)
            : base($"The type {stateType} does not implement {typeof(IState<>)}")
        {
            StateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
        }

        public Type StateType { get; }
    }
}
