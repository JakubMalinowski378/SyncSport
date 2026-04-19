using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Shared.Persistence;

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => ConvertToUtcUrl(v),
            v => ConvertFromUtcToLocal(v))
    {
    }

    private static DateTime ConvertToUtcUrl(DateTime v)
    {
        if (v.Kind == DateTimeKind.Utc)
        {
            return v;
        }

        if (v.Kind == DateTimeKind.Unspecified)
        {
            return DateTime.SpecifyKind(v.AddHours(-1), DateTimeKind.Utc);
        }

        return v.ToUniversalTime();
    }

    private static DateTime ConvertFromUtcToLocal(DateTime v)
    {
        return DateTime.SpecifyKind(v, DateTimeKind.Utc).AddHours(1);
    }
}
