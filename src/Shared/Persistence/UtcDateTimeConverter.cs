using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Shared.Persistence;

public class UtcDateTimeConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
{
    public UtcDateTimeConverter()
        : base(
            v => v.ToUniversalTime(),
            v => v.ToUniversalTime())
    {
    }
}
