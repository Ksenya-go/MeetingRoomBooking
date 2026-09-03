using MeetingBooking.Application.Common.Interfaces;

namespace MeetingBooking.Infrastructure.Notifications;


public class NoOpBookingNotifier : IBookingNotifier
{
    public Task NotifySlotBookedAsync(Guid resourceId, Guid timeSlotId, DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    } 
}