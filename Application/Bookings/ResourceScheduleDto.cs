namespace MeetingBooking.Application.Bookings;

public record TimeSlotStatusDto(Guid TimeSlotId, TimeOnly StartTime, TimeOnly EndTime, bool IsBooked);

public record ResourceScheduleDto(Guid ResourceId, string ResourceName, DateOnly Date, 
    IReadOnlyList<TimeSlotStatusDto> Slots);