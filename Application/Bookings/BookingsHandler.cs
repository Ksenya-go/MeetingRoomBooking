using FluentResults;
using Mediator;
using MeetingBooking.Application.Common.Errors;
using MeetingBooking.Application.Common.Interfaces;
using MeetingBooking.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MeetingBooking.Application.Bookings;

public class BookingsHandler : IRequestHandler<BookSlotCommand, Result<Guid>>, 
    IRequestHandler<GetResourceScheduleQuery, Result<ResourceScheduleDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IBookingNotifier _notifier;

    public BookingsHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IBookingNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
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