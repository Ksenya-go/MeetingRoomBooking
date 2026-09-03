using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Bookings;

public record GetResourceScheduleQuery(Guid ResourceId, DateOnly Date) : 
    IRequest<Result<ResourceScheduleDto>>;