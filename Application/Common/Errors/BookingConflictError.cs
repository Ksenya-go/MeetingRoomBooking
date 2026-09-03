using FluentResults;

namespace MeetingBooking.Application.Common.Errors;

public class BookingConflictError : Error
{
    public BookingConflictError(Guid resourceId, Guid timeSlotId, DateOnly date)
        : base($"Slot already booked for resource {resourceId} on {date:yyyy-MM-dd}.")
    {
        Metadata.Add("ResourceId", resourceId);
        Metadata.Add("TimeSlotId", timeSlotId);
        Metadata.Add("Date", date);
    }
}