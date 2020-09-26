namespace StateR
{
    public record AsyncErrorOccured<TLoadAction, TState, TResponse>(AsyncErrorState<TLoadAction, TState, TResponse> ErrorState) : IAction
        where TLoadAction : IAsyncAction<TResponse>
        where TState : AsyncState;
}