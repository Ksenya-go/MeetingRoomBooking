using FluentResults;
using Mediator;
using MeetingBooking.Application.Common.Interfaces;
using MeetingBooking.Application.Queries.TimeSlots;
using MeetingBooking.Application.TimeSlots.Commands;
using MeetingBooking.Domain;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace MeetingBooking.Application.TimeSlots;

public class TimeSlotsHandler :
    IRequestHandler<CreateTimeSlotCommand, Result<Guid>>,
    IRequestHandler<DeleteTimeSlotCommand, Result>,
    IRequestHandler<GetAllTimeSlotsQuery, Result<IPagedList<TimeSlotDto>>>
{
    private readonly IApplicationDbContext _db;

    public TimeSlotsHandler(IApplicationDbContext db)
    {
        _db = db;
    }
    /// <summary>Creates a new shared time slot. Admin-only.</summary>
    public async ValueTask<Result<Guid>> Handle(CreateTimeSlotCommand request, CancellationToken 
        cancellationToken)
    {
        var timeSlot = new TimeSlot(request.StartTime, request.EndTime);
        _db.TimeSlots.Add(timeSlot);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok(timeSlot.Id);
    }
    /// <summary>Deletes a time slot. Admin-only.</summary>
    public async ValueTask<Result> Handle(DeleteTimeSlotCommand request, CancellationToken 
        cancellationToken)
    {
        var timeSlot = await _db.TimeSlots.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (timeSlot is null)
        {
            return Result.Fail($"TimeSlot {request.Id} not found.");
        }
      
        _db.TimeSlots.Remove(timeSlot);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async ValueTask<Result<IPagedList<TimeSlotDto>>> Handle(GetAllTimeSlotsQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _db.TimeSlots
            .OrderBy(t => t.StartTime)
            .Select(t => new TimeSlotDto(t.Id, t.StartTime, t.EndTime));

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var slots = await baseQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var pagedList = new StaticPagedList<TimeSlotDto>(slots, request.PageNumber, request.PageSize, totalCount);

        return Result.Ok((IPagedList<TimeSlotDto>)pagedList);
    }
}