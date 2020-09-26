namespace StateR
{
    public record AsyncErrorOccured<TLoadAction, TState>(AsyncErrorState<TLoadAction, TState> ErrorState) : IAction
        where TLoadAction : IAction
        where TState : AsyncState;
}