using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Facilities.Infrastructure.Persistence.Configuration;

internal static class CollectionValueComparers
{
    public static ValueComparer<IReadOnlyCollection<TElement>> CreateCollectionComparer<TElement>()
        where TElement : notnull
        => new(
            (left, right) => ReferenceEquals(left, right) || (left != null && right != null && left.SequenceEqual(right)),
            collection => collection.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            collection => collection.ToList());
}