using FluentResults;
using Mediator;

namespace MeetingBooking.Application.TimeSlots.Commands;

public record DeleteTimeSlotCommand(Guid Id) : IRequest<Result>;