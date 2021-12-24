using System.Threading;
using System.Threading.Tasks;

namespace StateR.ActionHandlers.Hooks;

public interface IActionHandlerHooksCollection
{
    Task BeforeHandlerAsync<TAction>(IDispatchContext<TAction> context, IActionHandler<TAction> actionHandler, CancellationToken cancellationToken) where TAction : IAction;
    Task AfterHandlerAsync<TAction>(IDispatchContext<TAction> context, IActionHandler<TAction> actionHandler, CancellationToken cancellationToken) where TAction : IAction;
}
