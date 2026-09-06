using FluentResults;
using Mediator;
using MeetingBooking.Application.Bookings.Dtos;

namespace MeetingBooking.Application.Bookings.Queries;

public record GetResourceScheduleQuery(Guid ResourceId, DateOnly Date) : 
    IRequest<Result<ResourceScheduleDto>>;