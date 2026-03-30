using Facilities.Infrastructure.Persistence;
using Shared.Domain;
using Shared.Persistence;

namespace Facilities.Infrastructure;

internal sealed class FacilitiesRepository<TEntity, TId>(FacilitiesDbContext context)
    : Repository<TEntity, TId>(context)
    where TEntity : Entity<TId>;