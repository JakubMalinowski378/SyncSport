using Facilities.Application.Facilities.Common;
using FluentValidation;
using MediatR;

using Facilities.Application.Facilities.Commands.CreateFacility;

namespace Facilities.Application.Facilities.Commands.EditFacility;

public sealed record EditFacilityCommand(
    Guid FacilityId,
    string Name,
    string Address,
    int ReservationDuration,
    List<DailyHoursDto>? WeeklyHours = null,
    List<DateSpecificHoursDto>? CustomDateHours = null,
    List<ImageDto>? Images = null) : IRequest;

public sealed class EditFacilityCommandValidator : AbstractValidator<EditFacilityCommand>
{
    public EditFacilityCommandValidator()
    {
        RuleFor(x => x.FacilityId)
            .NotEmpty();

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

        RuleFor(x => x.Images)
            .Must(images => images is null || images.Count(i => i.IsMain) <= 1)
            .WithMessage("Only one image can be marked as main.");
    }
}

