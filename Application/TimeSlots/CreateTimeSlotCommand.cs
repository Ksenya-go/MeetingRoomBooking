using FluentResults;
using Mediator;

namespace MeetingBooking.Application.TimeSlots;

public record CreateTimeSlotCommand(TimeOnly StartTime, TimeOnly EndTime) : IRequest<Result<Guid>>;