namespace MeetingBooking.Application.Resources;

public record ResourceDto(Guid Id, string Name, string? Description, int Capacity, bool IsActive);