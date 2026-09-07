namespace MeetingBooking.Application.Common.Interfaces;


/// <summary>
/// Abstraction over real-time notifications.
/// </summary>
public interface IBookingNotifier
{
    Task NotifySlotBookedAsync(Guid resourceId, Guid timeSlotId, DateOnly date, 
        CancellationToken cancellationToken = default);
}