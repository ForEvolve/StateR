namespace StateR.Interceptors.Hooks;

public interface IAfterInterceptorHook
{
    Task AfterHandlerAsync<TAction>(IDispatchContext<TAction> context, IInterceptor<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
}
