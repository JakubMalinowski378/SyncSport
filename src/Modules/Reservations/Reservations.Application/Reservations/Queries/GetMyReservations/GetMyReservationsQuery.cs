using FluentValidation;
using MediatR;
using Shared.Behaviors;
using Shared.Pagination;
using System.Text.Json.Serialization;

namespace Reservations.Application.Reservations.Queries.GetMyReservations;

public sealed class GetMyReservationsQuery
    : IRequest<PagedResult<ReservationWithDetailsDto>>, IAuditable
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

internal sealed class GetMyReservationsQueryValidator : AbstractValidator<GetMyReservationsQuery>
{
    private static readonly int[] AllowedPageSizes = [5, 10, 15, 20, 25, 30];

    public GetMyReservationsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .Must(size => AllowedPageSizes.Contains(size))
            .WithMessage("PageSize must be one of: 5, 10, 15, 20, 25, 30.");
    }
}
