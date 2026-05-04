using MediatR;
using Shared.Persistence;
using Users.Application.Abstractions;
using Users.Application.Accounts.Common;
using Users.Domain.Entities;
using Users.Domain.Exceptions;

namespace Users.Application.Accounts.Commands.Refresh;

internal sealed class RefreshTokenCommandHandler(
    IRepository<User, Guid> userRepository,
    IRepository<Account, Guid> accountRepository,
    IJwtService jwtService)
    : IRequestHandler<RefreshTokenCommand, AuthenticationResponse>
{
    public async Task<AuthenticationResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.FirstOrDefaultAsync(a => a.RefreshToken == request.RefreshToken, ct: cancellationToken);

        if (account is null || account.RefreshTokenExpiryTime < DateTimeOffset.UtcNow)
        {
            throw new InvalidRefreshTokenException();
        }

        var user = await userRepository.GetByIdAsync(account.Id, ct: cancellationToken)
                   ?? throw new InvalidRefreshTokenException();

        var jwtToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        account.SetRefreshToken(refreshToken.Token, refreshToken.ExpiryTime);

        accountRepository.Update(account);
        await accountRepository.SaveChangesAsync(cancellationToken);

        return new AuthenticationResponse(jwtToken, refreshToken.Token);
    }
}
