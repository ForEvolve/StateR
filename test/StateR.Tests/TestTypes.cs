using StateR.Updaters;

namespace StateR;

public record class TestState1 : StateBase;
public record class TestState2 : StateBase;
public record class TestState3 : StateBase;

public record class InitialTestState1 : IInitialState<TestState1>
{
    public TestState1 Value => new();
}
public record class InitialTestState2 : IInitialState<TestState2>
{
    public TestState2 Value => new();
}
public record class InitialTestState3 : IInitialState<TestState3>
{
    public TestState3 Value => new();
}

public class NotAState { }
public class NotAnAction { }
public class NotAnUpdater { }

public record TestAction1 : IAction<TestState1>;

public class TestUpdaters : IUpdater<TestAction1, TestState1>
{
    public TestState1 Update(TestAction1 action, TestState1 state)
        => new();
}