namespace MeetingBooking.Domain;


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