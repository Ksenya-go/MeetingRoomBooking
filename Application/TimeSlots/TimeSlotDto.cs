namespace MeetingBooking.Application.TimeSlots;

public record TimeSlotDto(Guid Id, TimeOnly StartTime, TimeOnly EndTime);