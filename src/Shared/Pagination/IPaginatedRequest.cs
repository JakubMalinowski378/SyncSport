namespace Shared.Pagination;

public interface IPaginatedRequest
{
    int PageNumber { get; }
    int PageSize { get; }
}
