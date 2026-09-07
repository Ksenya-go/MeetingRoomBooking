using FluentResults;
using Mediator;
using MeetingBooking.Application.Bookings.Dtos;
using X.PagedList;

namespace MeetingBooking.Application.Bookings.Queries;
/// <summary>
/// Returns a paged list of bookings
/// </summary>
public record GetBookingsQuery(string UserId, bool IsAdmin, int PageNumber = 1, int PageSize = 6)
    : IRequest<Result<IPagedList<BookingListItemDto>>>;