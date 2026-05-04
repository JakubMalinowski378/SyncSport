using MediatR;
using Shared.Persistence;
using Users.Application.Abstractions;
using Users.Domain.Entities;
using Users.Domain.Exceptions;

namespace Users.Application.Accounts.Commands.PasswordReset;

internal sealed class ResetPasswordCommandHandler(
    IRepository<Account, Guid> accountRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.FirstOrDefaultAsync(
            a => a.PasswordResetToken == request.ResetToken,
            ct: cancellationToken);

        if (account is null || account.PasswordResetTokenExpiryTime < DateTimeOffset.UtcNow)
        {
            throw new InvalidPasswordResetTokenException();
        }

        var hashedPassword = passwordHasher.Hash(request.NewPassword);
        account.ChangePassword(hashedPassword);
        account.ClearPasswordResetToken();
        account.ClearRefreshToken();

        accountRepository.Update(account);
        await accountRepository.SaveChangesAsync(cancellationToken);
    }
}
