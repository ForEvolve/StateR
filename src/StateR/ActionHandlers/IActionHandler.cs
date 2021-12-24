namespace StateR.ActionHandlers;

public interface IActionHandler<TAction>
    where TAction : IAction
{
    Task HandleAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken);
}
