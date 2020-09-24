using MediatR;
namespace StateR
{
    public interface IAction : IRequest, INotification { }
}