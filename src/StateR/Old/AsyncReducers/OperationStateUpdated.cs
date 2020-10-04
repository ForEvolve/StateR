namespace StateR.Old
{
    public record OperationStateUpdated (AsyncOperationState NewRecordState) : IAction;
}