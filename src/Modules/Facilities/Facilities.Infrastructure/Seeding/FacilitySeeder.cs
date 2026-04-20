using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using Shared.Persistence;
using Shared.Seeding;

namespace Facilities.Infrastructure.Seeding;

internal sealed class FacilitySeeder(IRepository<Facility, FacilityId> repository) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await repository.AnyAsync(ct: cancellationToken))
        {
            return;
        }

        var standardHours = WeeklyOpeningHours.CreateUniform(
            new TimeSpan(8, 0, 0),
            new TimeSpan(22, 0, 0));

        var facility1 = Facility.Create(
            "Centralna Hala Sportowa",
            "ul. Glówna 123, Warszawa",
            60,
            standardHours);

        facility1.AddCourt("Główne Boisko do Koszykówki", "Parkiet");
        facility1.AddCourt("Kort Tenisowy 1", "Twarda");
        facility1.AddCourt("Kort Tenisowy 2", "Twarda");

        var weekendOnlyHours = WeeklyOpeningHours.Create(
        [
            DailyOpeningHours.CreateClosed(DayOfWeek.Monday),
            DailyOpeningHours.CreateClosed(DayOfWeek.Tuesday),
            DailyOpeningHours.CreateClosed(DayOfWeek.Wednesday),
            DailyOpeningHours.CreateClosed(DayOfWeek.Thursday),
            DailyOpeningHours.CreateClosed(DayOfWeek.Friday),
            DailyOpeningHours.Create(DayOfWeek.Saturday, new TimeSpan(9, 0, 0), new TimeSpan(20, 0, 0)),
            DailyOpeningHours.Create(DayOfWeek.Sunday, new TimeSpan(9, 0, 0), new TimeSpan(18, 0, 0))
        ]);

        var facility2 = Facility.Create(
            "Klub Tenisowy Sródmiescie",
            "ul. Wiazowa 456, Kraków",
            60,
            weekendOnlyHours);

        facility2.AddCourt("Kort A", "Maczka");
        facility2.AddCourt("Kort B", "Trawa");

        await repository.AddRangeAsync([facility1, facility2], cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
