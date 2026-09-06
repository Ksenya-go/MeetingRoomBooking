namespace MeetingBooking.Application.Bookings.Dtos;

public record BookingListItemDto(
    Guid Id,
    string ResourceName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    DateOnly Date,
    string UserId,
    string UserDisplayName);