using Shared.Domain;
using Shared.Persistence;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure;

internal sealed class UsersRepository<TEntity, TId>(UsersDbContext context)
    : Repository<TEntity, TId>(context)
    where TEntity : Entity<TId>;