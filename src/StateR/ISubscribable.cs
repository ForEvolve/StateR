using System;

namespace StateR;

public interface ISubscribable
{
    void Subscribe(Action stateHasChangedDelegate);
    void Unsubscribe(Action stateHasChangedDelegate);
    void Notify();
}
