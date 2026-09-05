namespace MeetingBooking.Application.Bookings;

public record BookingListItemDto(
    Guid Id,
    string ResourceName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    DateOnly Date,
    string UserId,
    string UserDisplayName);