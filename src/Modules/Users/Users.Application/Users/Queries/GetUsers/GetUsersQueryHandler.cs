using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Pagination;
using Shared.Persistence;
using Users.Application.Users.Queries.GetUser;
using Users.Domain.Entities;

namespace Users.Application.Users.Queries.GetUsers;

internal sealed class GetUsersQueryHandler(
    IRepository<User, Guid> userRepository)
    : IRequestHandler<GetUsersQuery, PagedResult<GetUserResponse>>
{
    public async Task<PagedResult<GetUserResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<User, bool>>? filter = null;
        
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            filter = u => u.Email.Value.ToLower().Contains(searchTerm) || 
                          u.Name.FirstName.ToLower().Contains(searchTerm) || 
                          u.Name.LastName.ToLower().Contains(searchTerm);
        }

        Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null;

        if (!string.IsNullOrWhiteSpace(request.SortColumn))
        {
            var sortOrder = string.IsNullOrWhiteSpace(request.SortOrder) ? "asc" : request.SortOrder.ToLower();

            orderBy = request.SortColumn switch
            {
                "Email" => sortOrder == "asc" ? q => q.OrderBy(u => u.Email.Value) : q => q.OrderByDescending(u => u.Email.Value),
                "FirstName" => sortOrder == "asc" ? q => q.OrderBy(u => u.Name.FirstName) : q => q.OrderByDescending(u => u.Name.FirstName),
                "LastName" => sortOrder == "asc" ? q => q.OrderBy(u => u.Name.LastName) : q => q.OrderByDescending(u => u.Name.LastName),
                _ => q => q.OrderBy(u => u.Email.Value)
            };
        }
        else
        {
            orderBy = q => q.OrderBy(u => u.Email.Value); // Default sort
        }

        var (items, totalCount) = await userRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            filter: filter,
            orderBy: orderBy,
            asNoTracking: true,
            ct: cancellationToken);

        var responseItems = items.Select(user => new GetUserResponse(
            user.Id,
            user.Email.Value,
            user.Name.FirstName,
            user.Name.LastName,
            user.Role.ToString(),
            user.IsActive,
            user.ManagedFacilityIds
        )).ToList();

        return new PagedResult<GetUserResponse>(
            responseItems,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
