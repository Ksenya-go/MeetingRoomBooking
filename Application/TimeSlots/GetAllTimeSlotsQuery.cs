using FluentResults;
using Mediator;

namespace MeetingBooking.Application.TimeSlots;

public record GetAllTimeSlotsQuery : IRequest<Result<IReadOnlyList<TimeSlotDto>>>;