namespace StateR
{
    public interface IAction { }
    public interface IActionHandler<TAction>
        where TAction : IAction
    {
        void Handle(DispatchContext<TAction> context);
    }
}