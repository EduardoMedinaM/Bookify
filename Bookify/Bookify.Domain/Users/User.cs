using Bookify.Domain.Abstractions;
using Bookify.Domain.Users.Events;

namespace Bookify.Domain.Users;

public sealed class User : Entity
{
    private readonly List<Role> _roles = [];

    public User(
        Guid id,
        FirstName firstName,
        LastName lastName,
        Email email) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    private User()
    {
        
    }

    /*
     *Entities are differentiated by a primary key (a unique identifier) 
     */

    public FirstName FirstName { get; private set; }
    public LastName LastName { get; private set; }
    public Email Email { get; private set; }
    public string IdentityId { get; private set; } = string.Empty;
    public IReadOnlyCollection<Role> Roles => _roles.ToList();


    public static User Create(FirstName firstName, LastName lastName, Email email)
    {
        /*
         * Benefits: 
         * - hiding ctor to avoid exposing entity details
         * - encapsulation
         * - to be able to introduce side effects that should not go on the ctor
         */
        User user = new(Guid.NewGuid(), firstName, lastName, email);

        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id));

        /*
         * You can add the role as another parameter 
         */
        user._roles.Add(Role.Registered);

        return user;
    }

    public void SetIdentityId(string identityId)
    {
        IdentityId = identityId;
    }
}
