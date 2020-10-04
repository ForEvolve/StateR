namespace StateR.Old
{
    public abstract record AsyncState : StateBase
    {
        public AsyncOperationState RecordState { get; init; }
    }
}