using FluentResults;
using Mediator;
using X.PagedList;

namespace MeetingBooking.Application.TimeSlots;

public record GetAllTimeSlotsQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<Result<IPagedList<TimeSlotDto>>>;