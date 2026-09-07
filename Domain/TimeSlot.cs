namespace MeetingBooking.Domain;
/// <summary>
/// A fixed daily bookable time range (e.g. 09:00–10:00), shared by all
/// resources rather than defined separately per room.
/// </summary>
public class TimeSlot
{
    public Guid Id { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    private TimeSlot() { } // EF Core

    public TimeSlot(TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("End time must be after start time.");
        }
     
        Id = Guid.NewGuid();
        StartTime = startTime;
        EndTime = endTime;
    }
}