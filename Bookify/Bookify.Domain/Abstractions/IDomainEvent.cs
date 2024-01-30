using MediatR;

namespace Bookify.Domain.Abstractions
{
    /// <summary>
    /// Represents all the domain events in our system
    /// something significant that occured in the domain
    /// and you want to notify others about just happened
    /// pub/sub pattern through MediatR
    /// </summary>
    public interface IDomainEvent : INotification
    {
    }
}
