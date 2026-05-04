using Pricing.Domain.Enums;
using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.Entities;

public partial class PriceRule
{
    protected PriceRule() { }

    private PriceRule(
        PriceRuleId id,
        RuleType type,
        decimal multiplier,
        DayOfWeek? dayOfWeek,
        TimeSpan? startTime,
        TimeSpan? endTime)
        : base(id)
    {
        Type = type;
        Multiplier = multiplier;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    public static PriceRule CreatePeakHoursRule(TimeSpan start, TimeSpan end, decimal multiplier)
    {
        return new PriceRule(PriceRuleId.New(), RuleType.PeakHoursMultiplier, multiplier, null, start, end);
    }

    public static PriceRule CreateWeekendRule(decimal multiplier)
    {
        return new PriceRule(PriceRuleId.New(), RuleType.WeekendMultiplier, multiplier, null, null, null);
    }

    public static PriceRule CreateDayRule(DayOfWeek day, decimal multiplier)
    {
        return new PriceRule(PriceRuleId.New(), RuleType.DayMultiplier, multiplier, day, null, null);
    }

    public Money Apply(Money currentPrice, DateTimeOffset reservationStart, DateTimeOffset reservationEnd)
    {
        bool applies = false;

        switch (Type)
        {
            case RuleType.WeekendMultiplier:
                if (reservationStart.DayOfWeek == System.DayOfWeek.Saturday || reservationStart.DayOfWeek == System.DayOfWeek.Sunday)
                {
                    applies = true;
                }
                break;

            case RuleType.DayMultiplier:
                if (reservationStart.DayOfWeek == DayOfWeek)
                {
                    applies = true;
                }
                break;

            case RuleType.PeakHoursMultiplier:
                var startHour = reservationStart.TimeOfDay;
                var endHour = reservationEnd.TimeOfDay;

                if (StartTime.HasValue && EndTime.HasValue)
                {
                    if (startHour < EndTime.Value && endHour > StartTime.Value)
                    {
                        applies = true;
                    }
                }
                break;
        }

        if (applies)
        {
            return currentPrice * Multiplier;
        }

        return currentPrice;
    }
}
