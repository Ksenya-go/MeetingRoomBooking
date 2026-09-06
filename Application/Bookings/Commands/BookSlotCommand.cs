using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Bookings.Commands;

public record BookSlotCommand(Guid ResourceId, Guid TimeSlotId, DateOnly Date) : IRequest<Result<Guid>>;