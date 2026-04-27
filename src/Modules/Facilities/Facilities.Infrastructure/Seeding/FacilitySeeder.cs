using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Shared.Persistence;
using Shared.Seeding;

namespace Facilities.Infrastructure.Seeding;

internal sealed class FacilitySeeder(
    IRepository<Facility, FacilityId> repository,
    IWebHostEnvironment environment) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        if (await repository.AnyAsync(ct: cancellationToken))
        {
            return;
        }

        var standardHours = WeeklyOpeningHours.Create(
        [
            DailyOpeningHours.Create(DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0)),
            DailyOpeningHours.Create(DayOfWeek.Tuesday, new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0)),
            DailyOpeningHours.Create(DayOfWeek.Wednesday, new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0)),
            DailyOpeningHours.Create(DayOfWeek.Thursday, new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0)),
            DailyOpeningHours.Create(DayOfWeek.Friday, new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0)),
            DailyOpeningHours.Create(DayOfWeek.Saturday, new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0)),
            DailyOpeningHours.Create(DayOfWeek.Sunday, new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0))
        ]);

        var facility1 = Facility.Create(
            "Centralna Hala Sportowa",
            "centralna-hala-sportowa",
            "ul. Glówna 123, Warszawa",
            60,
            standardHours);

        facility1.AddCourt("Główne Boisko do Koszykówki", "glowne-boisko-do-koszykowki", "Parkiet");
        facility1.AddCourt("Kort Tenisowy 1", "kort-tenisowy-1", "Twarda");
        facility1.AddCourt("Kort Tenisowy 2", "kort-tenisowy-2", "Twarda");

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
            "klub-tenisowy-srodmiescie",
            "ul. Wiazowa 456, Kraków",
            60,
            weekendOnlyHours);

        facility2.AddCourt("Kort A", "kort-a", "Maczka");
        facility2.AddCourt("Kort B", "kort-b", "Trawa");

        await repository.AddRangeAsync([facility1, facility2], cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
