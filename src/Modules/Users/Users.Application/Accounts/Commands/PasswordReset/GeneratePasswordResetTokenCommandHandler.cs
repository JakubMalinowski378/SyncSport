using MediatR;
using Shared.Persistence;
using System.Security.Cryptography;
using Users.Application.Abstractions;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.Accounts.Commands.PasswordReset;

internal sealed class GeneratePasswordResetTokenCommandHandler(
    IRepository<Account, Guid> accountRepository,
    IRepository<User, Guid> userRepository,
    IJwtService jwtService)
    : IRequestHandler<GeneratePasswordResetTokenCommand>
{
    public async Task Handle(GeneratePasswordResetTokenCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var account = await accountRepository.FirstOrDefaultAsync(a => a.Email == email, ct: cancellationToken);

        if (account is null)
        {
            return;
        }

        var user = await userRepository.GetByIdAsync(account.Id, ct: cancellationToken);
        if (user is null)
        {
            return;
        }

        var resetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiryTime = DateTimeOffset.UtcNow.AddMinutes(jwtService.GetPasswordResetTokenExpiryMinutes());

        account.SetPasswordResetToken(resetToken, expiryTime);
        user.RequestPasswordReset(resetToken);

        accountRepository.Update(account);
        userRepository.Update(user);

        await accountRepository.SaveChangesAsync(cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
