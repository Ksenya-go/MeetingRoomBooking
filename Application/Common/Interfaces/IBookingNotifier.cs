namespace MeetingBooking.Application.Common.Interfaces;

public interface IBookingNotifier
{
    Task NotifySlotBookedAsync(Guid resourceId, Guid timeSlotId, DateOnly date, CancellationToken cancellationToken = default);
}