using MediatR;
using Microsoft.Extensions.Logging;
using Notifications.Interfaces;
using Reservations.Shared;
using Reservations.Shared.Events;
using Users.Shared;

namespace Notifications.Handlers;

internal sealed class ReservationCreatedEventHandler(
    IReservationsModuleApi reservationsModuleApi,
    IUsersModuleApi usersModuleApi,
    ITemplateService templateService,
    IEmailSender emailSender,
    ILogger<ReservationCreatedEventHandler> logger) : INotificationHandler<ReservationCreatedEvent>
{
    public async Task Handle(ReservationCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event: Reservation created with Id {ReservationId}", notification.ReservationId);

        var reservation = await reservationsModuleApi.GetByIdAsync(notification.ReservationId, cancellationToken);
        if (reservation is null)
        {
            logger.LogWarning("Reservation with Id {ReservationId} not found.", notification.ReservationId);
            return;
        }

        var user = await usersModuleApi.GetUserAsync(reservation.UserId, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("User with Id {UserId} not found.", reservation.UserId);
            return;
        }

        var placeholders = new Dictionary<string, string>
        {
            { "UserName", user.FullName },
            { "CourtName", reservation.CourtName },
            { "StartTime", reservation.StartTime.ToString("g") },
            { "EndTime", reservation.EndTime.ToString("g") },
            { "Price", reservation.Price.ToString("F2") },
            { "Status", reservation.Status }
        };

        var emailBody = await templateService.RenderAsync("ReservationCreated", placeholders, cancellationToken);

        await emailSender.SendEmailAsync(
            to: user.Email,
            subject: "Reservation Confirmed - SyncSport",
            body: emailBody,
            cancellationToken: cancellationToken);
    }
}
