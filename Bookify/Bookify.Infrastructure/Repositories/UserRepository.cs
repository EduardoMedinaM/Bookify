using Bookify.Domain.Users;

namespace Bookify.Infrastructure.Repositories;

internal sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public override void Add(User user)
    {
        foreach (var role in user.Roles)
        {
            /*
             * Reads any pre-existing role record in the DB
             * and assigns it to the the user. It prevents duplicate keys.
             */
            DbContext.Attach(role);
        }

        DbContext.Add(user);
    }
}