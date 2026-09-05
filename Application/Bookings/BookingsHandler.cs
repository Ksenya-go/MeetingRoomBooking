using FluentResults;
using Mediator;
using MeetingBooking.Application.Common.Errors;
using MeetingBooking.Application.Common.Interfaces;
using MeetingBooking.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MeetingBooking.Application.Bookings;

public class BookingsHandler : IRequestHandler<BookSlotCommand, Result<Guid>>, 
    IRequestHandler<GetResourceScheduleQuery, Result<ResourceScheduleDto>>,
    IRequestHandler<GetBookingsQuery, Result<IReadOnlyList<BookingListItemDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IBookingNotifier _notifier;
    
    private readonly IUserLookupService _userLookup;


    public BookingsHandler(
     IApplicationDbContext db,
     ICurrentUserService currentUser,
     IBookingNotifier notifier,
     IUserLookupService userLookup)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
        _userLookup = userLookup;
    }

    public async ValueTask<Result<Guid>> Handle(BookSlotCommand request, CancellationToken cancellationToken)
    {
        var booking = new Booking(request.ResourceId, request.TimeSlotId, request.Date, _currentUser.UserId);

        _db.Bookings.Add(booking);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return Result.Fail(new BookingConflictError(request.ResourceId, request.TimeSlotId, request.Date));
        }

        await _notifier.NotifySlotBookedAsync(request.ResourceId, request.TimeSlotId, request.Date, cancellationToken);

        return Result.Ok(booking.Id);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sqlEx &&
               (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }
    public async ValueTask<Result<IReadOnlyList<BookingListItemDto>>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Bookings.AsQueryable();

        if (!request.IsAdmin)
        {
            query = query.Where(b => b.UserId == request.UserId);
        }

        var rows = await query
            .OrderByDescending(b => b.Date)
            .Join(_db.Resources, b => b.ResourceId, r => r.Id, (b, r) => new { b, r })
            .Join(_db.TimeSlots, x => x.b.TimeSlotId, t => t.Id, (x, t) =>
                new { x.b.Id, x.r.Name, t.StartTime, t.EndTime, x.b.Date, x.b.UserId })
            .ToListAsync(cancellationToken);

        var displayNames = await _userLookup.GetDisplayNamesAsync(
            rows.Select(r => r.UserId), cancellationToken);

        var bookings = rows
            .Select(r => new BookingListItemDto(
                r.Id, r.Name, r.StartTime, r.EndTime, r.Date, r.UserId,
                displayNames.GetValueOrDefault(r.UserId, r.UserId)))
            .ToList();

        return Result.Ok((IReadOnlyList<BookingListItemDto>)bookings);
    }
    public async ValueTask<Result<ResourceScheduleDto>> Handle(GetResourceScheduleQuery request, CancellationToken cancellationToken)
    {
        var resource = await _db.Resources
            .FirstOrDefaultAsync(r => r.Id == request.ResourceId, cancellationToken);

        if (resource is null)
        {
            return Result.Fail($"Resource {request.ResourceId} not found.");
        }

        var timeSlots = await _db.TimeSlots
            .OrderBy(t => t.StartTime)
            .ToListAsync(cancellationToken);

        var bookedTimeSlotIds = await _db.Bookings
            .Where(b => b.ResourceId == request.ResourceId && b.Date == request.Date)
            .Select(b => b.TimeSlotId)
            .ToListAsync(cancellationToken);

        var slots = timeSlots
            .Select(t => new TimeSlotStatusDto(t.Id, t.StartTime, t.EndTime, bookedTimeSlotIds.Contains(t.Id)))
            .ToList();

        return Result.Ok(new ResourceScheduleDto(resource.Id, resource.Name, request.Date, slots));
    }

    





}