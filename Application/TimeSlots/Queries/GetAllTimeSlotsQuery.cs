using FluentResults;
using Mediator;
using MeetingBooking.Application.TimeSlots;
using X.PagedList;

namespace MeetingBooking.Application.Queries.TimeSlots;

public record GetAllTimeSlotsQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<Result<IPagedList<TimeSlotDto>>>;