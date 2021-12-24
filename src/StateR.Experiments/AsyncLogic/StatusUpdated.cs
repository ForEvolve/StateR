namespace StateR.AsyncLogic;

public record StatusUpdated<TState>(AsyncOperationStatus status) : IAction
    where TState : AsyncState;
