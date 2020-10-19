using StateR.Internal;
using Microsoft.JSInterop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StateR.Reducers;
using System.Threading;

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

    public class ReduxDevToolsInterop : IDisposable, IReducersMiddleware
    {
        public bool DevToolsBrowserPluginDetected { get; private set; }
        private readonly Func<Task> _onCommit;
        private readonly Func<JumpToStateCallback, Task> _onJumpToState;
        private readonly IJSRuntime _jsRuntime;
        private readonly DotNetObjectReference<ReduxDevToolsInterop> _dotNetRef;
        private readonly IStore _store;
        private bool _isInitializing;

        private readonly DevToolsStateCollection _states;

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
            _isInitializing = true;
            try
            {
                await InvokeFluxorDevToolsMethodAsync("init", _dotNetRef);
            }
            finally
            {
                _isInitializing = false;
            }
        }

        [JSInvokable("DevToolsCallback")]
        public async Task DevToolsCallback(string messageAsJson)
        {
            if (string.IsNullOrWhiteSpace(messageAsJson))
                return;

            var message = JsonSerializer.Deserialize<BaseCallbackObject>(messageAsJson);
            switch (message?.payload?.type)
            {
                case "detected":
                    DevToolsBrowserPluginDetected = true;
                    break;

                case "COMMIT":
                    Console.WriteLine($"COMMIT | {messageAsJson}");
                    //Func<Task> commit = _onCommit;
                    //if (commit != null)
                    //{
                    //    Task task = commit();
                    //    if (task != null)
                    //        await task;
                    //}
                    break;

                case "JUMP_TO_STATE":
                    Console.WriteLine($"JUMP_TO_STATE | {messageAsJson}");
                    break;
                case "JUMP_TO_ACTION":
                    Console.WriteLine($"JUMP_TO_ACTION | {messageAsJson}");

                    //Func<JumpToStateCallback, Task> jumpToState = _onJumpToState;
                    //if (jumpToState != null)
                    //{
                    //    var callbackInfo = JsonSerializer.Deserialize<JumpToStateCallback>(messageAsJson);
                    //    Task task = jumpToState(callbackInfo);
                    //    if (task != null)
                    //        await task;
                    //}
                    break;
            }
        }

        void IDisposable.Dispose()
        {
            _dotNetRef.Dispose();
        }

        private async Task InvokeFluxorDevToolsMethodAsync(string identifier, object arg)
        {
            if (!DevToolsBrowserPluginDetected && !_isInitializing)
            {
                return;
            }

            var states = new Dictionary<string, object>();
            foreach (var s in _states)
            {
                var value = _store.GetStateValue(s);
                var name = s.GetStateName();
                states.Add(name, value);
            }

            if (!_isInitializing)
            {
                arg = JsonSerializer.Serialize(arg);
            }
            await _jsRuntime.InvokeAsync<object>($"__StateRDevTools__.{identifier}", CancellationToken.None, arg, states);
        }

        public Task BeforeReducersAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase
        {
            return Task.CompletedTask;
        }

        public Task BeforeReducerAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IReducer<TAction, TState> reducer, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase
        {
            return Task.CompletedTask;
        }

        public Task AfterReducerAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IReducer<TAction, TState> reducer, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase
            => InvokeFluxorDevToolsMethodAsync("dispatch", new ActionInfo(context.Action));
        

        public Task AfterReducersAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase
        {
            return Task.CompletedTask;
        }

        public Task BeforeNotifyAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase
        {
            return Task.CompletedTask;
        }

        public Task AfterNotifyAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase
        {
            return Task.CompletedTask;
        }
    }

    public class BaseCallbackObject<TPayload>
        where TPayload : BasePayload
    {
#pragma warning disable IDE1006 // Naming Styles
        public string type { get; set; }
        public TPayload payload { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }

    public class BaseCallbackObject : BaseCallbackObject<BasePayload> { }

    public class BasePayload
    {
#pragma warning disable IDE1006 // Naming Styles
        public string type { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }

    public class JumpToStateCallback : BaseCallbackObject<JumpToStatePayload>
    {
#pragma warning disable IDE1006 // Naming Styles
        public string state { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
    public class JumpToStatePayload : BasePayload
    {
#pragma warning disable IDE1006 // Naming Styles
        public int index { get; set; }
        public int actionId { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
    public class ActionInfo
    {
#pragma warning disable IDE1006 // Naming Styles
        public string type { get; }
#pragma warning restore IDE1006 // Naming Styles
        public object Payload { get; }

        public ActionInfo(object action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            type = action.GetType().GetStatorName();
            Payload = action;
        }
    }

    public static class StoreExtensions
    {
        public static object GetStateValue(this IStore store, Type state)
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
            var getState = store.GetType().GetMethod(nameof(IStore.GetState));
            var stateArgs = state.GetGenericArguments()[0];
            var genericMethod = getState.MakeGenericMethod(stateArgs);
            var result = genericMethod.Invoke(store, null);
            return result;
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
