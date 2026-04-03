using MediatR;
using Microsoft.Extensions.Options;
using Users.Shared.Events;
using Notifications.Interfaces;
using Notifications.Options;

namespace Notifications.Handlers;

internal sealed class PasswordResetRequestedEventHandler(
    IEmailSender emailSender,
    ITemplateService templateService,
    IOptions<FrontendOptions> frontendOptions) 
    : INotificationHandler<PasswordResetRequestedEvent>
{
    public async Task Handle(PasswordResetRequestedEvent notification, CancellationToken cancellationToken)
    {
        var frontendUrl = frontendOptions.Value.Url.TrimEnd('/');
        var placeholders = new Dictionary<string, string>
        {
            { "UserName", notification.FirstName },
            { "ResetLink", $"{frontendUrl}/reset-password?token={notification.ResetToken}" }
        };

        var body = await templateService.RenderAsync("PasswordReset", placeholders, cancellationToken);

        await emailSender.SendEmailAsync(
            to: notification.Email,
            subject: "Password Reset Request",
            body: body,
            cancellationToken: cancellationToken);
    }
}
