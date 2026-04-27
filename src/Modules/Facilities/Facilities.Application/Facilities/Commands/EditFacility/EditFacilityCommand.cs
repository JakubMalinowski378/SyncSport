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
    public List<string>? RemovedImageUrls { get; set; }
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

        RuleForEach(x => x.RemovedImageUrls)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Each removed image URL must be a valid absolute URI.")
            .When(x => x.RemovedImageUrls is not null && x.RemovedImageUrls.Count > 0);

        RuleFor(x => x.MainImageIndex)
            .Must(index => !index.HasValue || index.Value >= 0)
            .WithMessage("MainImageIndex must be greater than or equal to 0.")
            .When(x => x.MainImageIndex.HasValue);
    }
}

