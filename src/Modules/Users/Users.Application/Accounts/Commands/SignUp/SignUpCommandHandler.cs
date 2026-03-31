using MediatR;
using Shared.Persistence;
using Users.Application.Abstractions;
using Users.Application.Accounts.Common;
using Users.Domain.Entities;
using Users.Domain.Exceptions;
using Users.Domain.ValueObjects;

namespace Users.Application.Accounts.Commands.SignUp;

internal sealed class SignUpCommandHandler(
    IPasswordHasher passwordHasher,
    IRepository<User, Guid> userRepository,
    IRepository<Account, Guid> accountRepository,
    IJwtService jwtService)
    : IRequestHandler<SignUpCommand, AuthenticationResponse>
{
    public async Task<AuthenticationResponse> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        var isEmailTaken = await accountRepository.AnyAsync(a => a.Email == email, cancellationToken);

        if (isEmailTaken)
        {
            throw new EmailAlreadyTakenException(email.Value);
        }

        var hashedPassword = passwordHasher.Hash(request.Password);

        var id = Guid.NewGuid();
        var account = Account.Create(id, email, hashedPassword);
        var user = User.Register(id, email, FullName.Create(request.FirstName, request.LastName));

        var jwtToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        account.SetRefreshToken(refreshToken.Token, refreshToken.ExpiryTime);

        await accountRepository.AddAsync(account, cancellationToken);
        await userRepository.AddAsync(user, cancellationToken);

        await accountRepository.SaveChangesAsync(cancellationToken);

        return new AuthenticationResponse(jwtToken, refreshToken.Token);
    }
}
