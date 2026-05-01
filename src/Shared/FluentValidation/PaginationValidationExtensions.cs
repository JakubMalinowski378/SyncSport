using FluentValidation;
using Shared.Pagination;

namespace Shared.FluentValidation;

public static class PaginationValidationExtensions
{
    private static readonly int[] AllowedPageSizes = [5, 10, 15, 20, 25, 30];

    public static void AddPaginationRules<T>(this AbstractValidator<T> validator)
        where T : IPaginatedRequest
    {
        validator.RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageNumber must be greater than or equal to 1.");

        validator.RuleFor(x => x.PageSize)
            .Must(size => AllowedPageSizes.Contains(size))
            .WithMessage($"PageSize must be one of: {string.Join(", ", AllowedPageSizes)}.");
    }
}
