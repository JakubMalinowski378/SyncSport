using FluentValidation;
using MediatR;

namespace Pricing.Application.Tariffs.Commands.EditTariff;

public record EditTariffCommand(
    Guid FacilityId,
    decimal BaseHourlyRate,
    IReadOnlyCollection<EditCourtRateOverrideRequest>? CourtOverrides
) : IRequest<bool>;

public record EditCourtRateOverrideRequest(
    Guid CourtId,
    decimal HourlyRate
);

public sealed class EditTariffCommandValidator : AbstractValidator<EditTariffCommand>
{
    public EditTariffCommandValidator()
    {
        RuleFor(x => x.FacilityId)
            .NotEmpty();

        RuleFor(x => x.BaseHourlyRate)
            .GreaterThanOrEqualTo(0);

        RuleForEach(x => x.CourtOverrides)
            .ChildRules(overrideRules =>
            {
                overrideRules.RuleFor(x => x.CourtId)
                    .NotEmpty();

                overrideRules.RuleFor(x => x.HourlyRate)
                    .GreaterThanOrEqualTo(0);
            });

        RuleFor(x => x.CourtOverrides)
            .Must(overrides => overrides is null || overrides.Select(x => x.CourtId).Distinct().Count() == overrides.Count)
            .WithMessage("Court overrides must contain unique court IDs.");
    }
}
