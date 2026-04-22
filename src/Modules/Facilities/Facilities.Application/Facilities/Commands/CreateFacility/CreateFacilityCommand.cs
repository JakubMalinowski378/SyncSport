using FluentValidation;
using MediatR;

namespace Facilities.Application.Facilities.Commands.CreateFacility;

public record DailyHoursDto(DayOfWeek DayOfWeek, TimeSpan OpenTime, TimeSpan CloseTime, bool IsClosed);
public record DateSpecificHoursDto(DateOnly Date, TimeSpan OpenTime, TimeSpan CloseTime, bool IsClosed);

public sealed record CreateFacilityCommand(
    string Name,
    string Address,
    int ReservationDuration,
    List<DailyHoursDto>? WeeklyHours = null,
    List<DateSpecificHoursDto>? CustomDateHours = null,
    List<string>? Images = null) : IRequest<Guid>;       

public sealed class CreateFacilityCommandValidator : AbstractValidator<CreateFacilityCommand>
{
    public CreateFacilityCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.ReservationDuration)
            .GreaterThan(0);

        RuleForEach(x => x.WeeklyHours)
            .ChildRules(daily =>
            {
                daily.RuleFor(x => x.OpenTime)
                    .LessThan(x => x.CloseTime)
                    .When(x => !x.IsClosed)
                    .WithMessage("OpenTime must be before CloseTime.");
            });

        RuleForEach(x => x.CustomDateHours)
            .ChildRules(custom =>
            {
                custom.RuleFor(x => x.OpenTime)
                    .LessThan(x => x.CloseTime)
                    .When(x => !x.IsClosed)
                    .WithMessage("OpenTime must be before CloseTime.");
            });
    }
}
