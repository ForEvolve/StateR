using StateR.AfterEffects;
using StateR.Interceptors;
using StateR.Reducers;

namespace StateR
{
    public interface IStatorMiddleware : IInterceptorsMiddleware, IActionHandlerMiddleware, IReducersMiddleware, IAfterEffectsMiddleware
    {
    }
}
