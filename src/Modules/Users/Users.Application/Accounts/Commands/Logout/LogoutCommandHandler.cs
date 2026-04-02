using MediatR;
using Shared.Persistence;
using Users.Domain.Entities;

namespace Users.Application.Accounts.Commands.Logout;

internal sealed class LogoutCommandHandler(
    IRepository<Account, Guid> accountRepository)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.FirstOrDefaultAsync(
            a => a.RefreshToken == request.RefreshToken,
            ct: cancellationToken);

        if (account is null)
        {
            return;
        }

        account.ClearRefreshToken();

        accountRepository.Update(account);
        await accountRepository.SaveChangesAsync(cancellationToken);
    }
}
