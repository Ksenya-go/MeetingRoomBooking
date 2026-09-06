using MeetingBooking.Application.Common.Interfaces;

namespace MeetingBooking.Tests.TestDoubles;

public class NoOpBookingNotifier : IBookingNotifier
{
    public Task NotifySlotBookedAsync(Guid resourceId, Guid timeSlotId, DateOnly date, 
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}