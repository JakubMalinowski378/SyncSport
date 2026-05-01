using FluentValidation;
using MediatR;
using Reservations.Domain.Enums;
using Shared.Behaviors;
using Shared.FluentValidation;
using Shared.Pagination;
using System.Text.Json.Serialization;

namespace Reservations.Application.Reservations.Queries.GetMyReservations;

public sealed class GetMyReservationsQuery
    : IRequest<PagedResult<ReservationWithDetailsDto>>, IAuditable, IPaginatedRequest
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }

    public ReservationStatus? Status { get; set; }
    public string? CourtName { get; set; }
    public string? FacilityName { get; set; }
}

public static class GetMyReservationsSortFields
{
    public const string Date = "date";
    public const string Status = "status";
    public const string Price = "price";
}

public static class SortDirection
{
    public const string Asc = "asc";
    public const string Desc = "desc";
}

internal sealed class GetMyReservationsQueryValidator : AbstractValidator<GetMyReservationsQuery>
{
    private static readonly string[] AllowedSortFields = [
        GetMyReservationsSortFields.Date,
        GetMyReservationsSortFields.Status,
        GetMyReservationsSortFields.Price
    ];
    private static readonly string[] AllowedSortDirections = [SortDirection.Asc, SortDirection.Desc];

    public GetMyReservationsQueryValidator()
    {
        this.AddPaginationRules();

        RuleFor(x => x.SortBy)
            .Must(field => field is null || AllowedSortFields.Contains(field))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}.");

        RuleFor(x => x.SortDirection)
            .Must(dir => dir is null || AllowedSortDirections.Contains(dir))
            .WithMessage($"SortDirection must be one of: {string.Join(", ", AllowedSortDirections)}.");

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue);
    }
}
