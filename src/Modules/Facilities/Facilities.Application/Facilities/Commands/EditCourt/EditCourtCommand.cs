using Facilities.Application.Facilities.Common;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.FluentValidation;

namespace Facilities.Application.Facilities.Commands.EditCourt;

public sealed class EditCourtCommand : IRequest
{
    public Guid FacilityId { get; set; }
    public Guid CourtId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? OverrideReservationDuration { get; set; }
    public IFormFileCollection? Images { get; set; }
    public int? MainImageIndex { get; set; }
}

public sealed class EditCourtCommandValidator : AbstractValidator<EditCourtCommand>
{
    public EditCourtCommandValidator()
    {
        RuleFor(x => x.FacilityId)
            .NotEmpty();

        RuleFor(x => x.CourtId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.OverrideReservationDuration)
            .GreaterThan(0)
            .When(x => x.OverrideReservationDuration.HasValue)
            .WithMessage("Override reservation duration must be greater than zero if provided.");

        RuleFor(x => x.Images)
            .ValidateImageFormFiles();

        RuleFor(x => x.MainImageIndex)
            .Must((cmd, index) => !index.HasValue || (cmd.Images is not null && index.Value >= 0 && index.Value < cmd.Images.Count))
            .WithMessage("MainImageIndex must be valid for the provided images.")
            .When(x => x.MainImageIndex.HasValue);
    }
}