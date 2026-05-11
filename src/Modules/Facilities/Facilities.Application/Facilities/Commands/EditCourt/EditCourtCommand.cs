using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.FluentValidation;

namespace Facilities.Application.Facilities.Commands.EditCourt;

public sealed class EditCourtCommand : IRequest
{
    public Guid CourtId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? OverrideReservationDuration { get; set; }
    public IFormFileCollection? Images { get; set; }
    public List<string>? RemovedImageUrls { get; set; }
}

public sealed class EditCourtCommandValidator : AbstractValidator<EditCourtCommand>
{
    public EditCourtCommandValidator()
    {
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

        RuleForEach(x => x.RemovedImageUrls)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Each removed image URL must be a valid absolute URI.")
            .When(x => x.RemovedImageUrls is not null && x.RemovedImageUrls.Count > 0);
    }
}