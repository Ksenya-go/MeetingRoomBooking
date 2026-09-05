using FluentResults;
using Mediator;
using X.PagedList;

namespace MeetingBooking.Application.Bookings;

public record GetBookingsQuery(string UserId, bool IsAdmin, int PageNumber = 1, int PageSize = 6)
    : IRequest<Result<IPagedList<BookingListItemDto>>>;