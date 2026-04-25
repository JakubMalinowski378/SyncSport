using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.FluentValidation;

namespace Facilities.Application.Facilities.Commands.EditFacility;

public sealed class EditFacilityCommand : IRequest
{
    public Guid FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int ReservationDuration { get; set; }
    public string? WeeklyHours { get; set; }
    public string? CustomDateHours { get; set; }
    public IFormFileCollection? Images { get; set; }
    public int? MainImageIndex { get; set; }
}

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

        RuleFor(x => x.Images)
            .ValidateImageFormFiles();

        RuleFor(x => x.MainImageIndex)
            .Must((cmd, index) => !index.HasValue || (cmd.Images is not null && index.Value >= 0 && index.Value < cmd.Images.Count))
            .WithMessage("MainImageIndex must be valid for the provided images.")
            .When(x => x.MainImageIndex.HasValue);
    }
}

