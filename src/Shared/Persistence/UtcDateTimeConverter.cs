using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Time;

namespace Shared.Persistence;

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => ConvertToUtc(v),
            v => ConvertFromUtc(v))
    {
    }

    private static DateTime ConvertToUtc(DateTime v)
    {
        if (v.Kind == DateTimeKind.Utc)
            return v;

        if (v.Kind == DateTimeKind.Unspecified)
            return PolishTimeProvider.ConvertPolishLocalToUtc(v);

        return v.ToUniversalTime();
    }

    private static DateTime ConvertFromUtc(DateTime v)
    {
        return DateTime.SpecifyKind(v, DateTimeKind.Utc);
    }
}
