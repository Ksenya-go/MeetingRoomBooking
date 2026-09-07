using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MeetingBooking.Domain;

/// <summary>
/// A booking of a specific Resource's TimeSlot on a specific Date, made
///by a specific user.Uniqueness of (ResourceId, TimeSlotId, Date) is
///enforced by a database index (see BookingConfiguration)
/// </summary>

public class Booking
{
    public Guid Id { get; private set; }
    public Guid ResourceId { get; private set; }
    public Guid TimeSlotId { get; private set; }
    public DateOnly Date { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    private Booking() { } // EF Core

    public Booking(Guid resourceId, Guid timeSlotId, DateOnly date, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }
            

        Id = Guid.NewGuid();
        ResourceId = resourceId;
        TimeSlotId = timeSlotId;
        Date = date;
        UserId = userId;
        CreatedAtUtc = DateTime.UtcNow;
    }
}