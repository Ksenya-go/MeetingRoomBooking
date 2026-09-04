using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Bookings;

public record GetBookingsQuery(string UserId, bool IsAdmin) : IRequest<Result<IReadOnlyList<BookingListItemDto>>>;