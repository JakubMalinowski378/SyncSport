using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Shared.Behaviors;

public sealed class UserContextBehavior<TRequest, TResponse>(
    IHttpContextAccessor httpContextAccessor) 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IAuditable userRequest)
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                              ?? httpContextAccessor.HttpContext?.User?.FindFirst("sub");

            if (userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                userRequest.UserId = userId;
            }
        }

        return await next();
    }
}