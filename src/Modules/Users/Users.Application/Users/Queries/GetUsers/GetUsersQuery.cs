using FluentValidation;
using MediatR;
using Shared.Pagination;
using Users.Application.Users.Queries.GetUser;

namespace Users.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortColumn = null,
    string? SortOrder = null) : IRequest<PagedResult<GetUserResponse>>;

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    private static readonly string[] AllowedSortColumns = ["Email", "FirstName", "LastName"];
    private static readonly string[] AllowedSortOrders = ["asc", "desc"];

    public GetUsersQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
        
        RuleFor(x => x.SortColumn)
            .Must(x => x is null || AllowedSortColumns.Contains(x))
            .WithMessage($"Sort column must be one of: {string.Join(", ", AllowedSortColumns)}");

        RuleFor(x => x.SortOrder)
            .Must(x => x is null || AllowedSortOrders.Contains(x.ToLower()))
            .WithMessage($"Sort order must be one of: {string.Join(", ", AllowedSortOrders)}");
    }
}
