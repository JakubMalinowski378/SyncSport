using MediatR;
using Shared.Persistence;
using Users.Application.Abstractions;
using Users.Application.Accounts.Common;
using Users.Domain.Entities;
using Users.Domain.Exceptions;
using Users.Domain.ValueObjects;

namespace Users.Application.Accounts.Commands.SignIn;

internal sealed class SignInCommandHandler(
    IPasswordHasher passwordHasher,
    IRepository<User, Guid> userRepository,
    IRepository<Account, Guid> accountRepository,
    IJwtService jwtService)
    : IRequestHandler<SignInCommand, AuthenticationResponse>
{
    public async Task<AuthenticationResponse> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var account = await accountRepository.FirstOrDefaultAsync(a => a.Email == email, ct: cancellationToken);

        if (account is null || !passwordHasher.Verify(request.Password, account.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var user = await userRepository.GetByIdAsync(account.Id, ct: cancellationToken)
                   ?? throw new InvalidCredentialsException();

        var jwtToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        account.SetRefreshToken(refreshToken.Token, refreshToken.ExpiryTime);   

        accountRepository.Update(account);
        await accountRepository.SaveChangesAsync(cancellationToken);

        return new AuthenticationResponse(jwtToken, refreshToken.Token);        
    }
}
