using MediatR;
using Shared.Domain.Exceptions;
using Shared.Persistence;
using Users.Application.Abstractions;
using Users.Application.Accounts.Common;
using Users.Domain.Entities;
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
            throw new DomainException("Email is already taken.");
        }

        var hashedPassword = passwordHasher.Hash(request.Password);

        var account = Account.Create(email, hashedPassword);
        var user = User.Register(email, FullName.Create(request.FirstName, request.LastName));

        await accountRepository.AddAsync(account, cancellationToken);
        await userRepository.AddAsync(user, cancellationToken);

        await accountRepository.SaveChangesAsync(cancellationToken);

        var jwtToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        return new AuthenticationResponse(jwtToken, refreshToken);
    }
}
