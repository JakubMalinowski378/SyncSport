using Pricing.Infrastructure.Persistence;
using Shared.Domain;
using Shared.Persistence;

namespace Pricing.Infrastructure;

internal sealed class PricingRepository<TEntity, TId>(PricingDbContext context)
    : Repository<TEntity, TId>(context)
    where TEntity : Entity<TId>;