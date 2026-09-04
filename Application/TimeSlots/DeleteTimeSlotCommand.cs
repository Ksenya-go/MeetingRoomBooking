using FluentResults;
using Mediator;

namespace MeetingBooking.Application.TimeSlots;

public record DeleteTimeSlotCommand(Guid Id) : IRequest<Result>;