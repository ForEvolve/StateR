namespace StateR.AsyncLogic
{
    public record StatusUpdated(AsyncOperationStatus status) : IAction;
}
