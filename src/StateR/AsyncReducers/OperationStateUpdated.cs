namespace StateR
{
    public record OperationStateUpdated (AsyncOperationState NewRecordState) : IAction;
}