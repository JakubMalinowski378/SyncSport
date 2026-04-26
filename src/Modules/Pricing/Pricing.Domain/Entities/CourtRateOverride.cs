using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.Entities;

public sealed class CourtRateOverride
{
    public Guid CourtId { get; private set; }
    public Money HourlyRate { get; private set; } = default!;

    private CourtRateOverride() { }

    private CourtRateOverride(Guid courtId, Money hourlyRate)
    {
        if (courtId == Guid.Empty)
        {
            throw new ArgumentException("Court ID cannot be empty.");
        }

        CourtId = courtId;
        HourlyRate = hourlyRate;
    }

    public static CourtRateOverride Create(Guid courtId, Money hourlyRate)
    {
        return new CourtRateOverride(courtId, hourlyRate);
    }

    public void UpdateHourlyRate(Money hourlyRate)
    {
        HourlyRate = hourlyRate;
    }
}