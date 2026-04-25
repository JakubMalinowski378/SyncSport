using Facilities.Application.Facilities.Common;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.FluentValidation;

namespace Facilities.Application.Facilities.Commands.CreateCourt;

public sealed class CreateCourtCommand : IRequest<Guid>
{
    public Guid FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SurfaceType { get; set; } = string.Empty;
    public int? OverrideReservationDuration { get; set; }
    public IFormFileCollection? Images { get; set; }
    public int? MainImageIndex { get; set; }
}

public sealed class CreateCourtCommandValidator : AbstractValidator<CreateCourtCommand>
{
    public CreateCourtCommandValidator()
    {
        RuleFor(x => x.FacilityId)
            .NotEmpty()
            .WithMessage("Facility ID cannot be empty.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Court name cannot be empty and must not exceed 100 characters.");

        RuleFor(x => x.SurfaceType)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Surface type cannot be empty and must not exceed 50 characters.");

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

