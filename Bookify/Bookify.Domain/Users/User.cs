using Bookify.Domain.Abstractions;
using Bookify.Domain.Users.Events;

namespace Bookify.Domain.Users;

public sealed class User(Guid id, FirstName firstName, LastName lastName, Email email) : Entity(id)
{

    /*
     *Entities are differenciated by a primary key (a unique identifier) 
     */

    public FirstName FirstName { get; private set; } = firstName;
    public LastName LastName { get; private set; } = lastName;
    public Email Email { get; private set; } = email;


    public static User Create(FirstName firstName, LastName lastName, Email email)
    {
        /*
         * Benefits: 
         * - hidding ctor to avoid exposing entity details
         * - encapsulation
         * - to be able to introduce side effects that should not go on the ctor
         */
        User user = new(Guid.NewGuid(), firstName, lastName, email);

        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id));

        return user;
    }
}
