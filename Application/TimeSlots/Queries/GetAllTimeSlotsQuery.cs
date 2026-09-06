using FluentResults;
using Mediator;

namespace MeetingBooking.Application.TimeSlots.Queries;

public record GetAllTimeSlotsQuery : IRequest<Result<IReadOnlyList<TimeSlotDto>>>;