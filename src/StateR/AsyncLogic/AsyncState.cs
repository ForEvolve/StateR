namespace StateR.AsyncLogic
{
    public abstract record AsyncState : StateBase
    {
        public AsyncOperationStatus Status { get; init; }
    }
}
