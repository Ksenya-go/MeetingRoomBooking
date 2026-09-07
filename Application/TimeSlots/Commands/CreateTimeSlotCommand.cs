using FluentResults;
using Mediator;

namespace MeetingBooking.Application.TimeSlots.Commands;

public record CreateTimeSlotCommand(TimeOnly StartTime, TimeOnly EndTime) : IRequest<Result<Guid>>;