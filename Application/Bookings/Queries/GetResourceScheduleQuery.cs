using FluentResults;
using Mediator;
using MeetingBooking.Application.Bookings.Dtos;

namespace MeetingBooking.Application.Bookings.Queries;
/// Returns a resource's time slots for a given date, each flagged as
/// free or booked.
public record GetResourceScheduleQuery(Guid ResourceId, DateOnly Date) : 
    IRequest<Result<ResourceScheduleDto>>;