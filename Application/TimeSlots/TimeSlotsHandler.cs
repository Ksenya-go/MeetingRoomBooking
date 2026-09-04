using FluentResults;
using Mediator;
using MeetingBooking.Application.Common.Interfaces;
using MeetingBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace MeetingBooking.Application.TimeSlots;

public class TimeSlotsHandler :
    IRequestHandler<CreateTimeSlotCommand, Result<Guid>>,
    IRequestHandler<DeleteTimeSlotCommand, Result>,
    IRequestHandler<GetAllTimeSlotsQuery, Result<IReadOnlyList<TimeSlotDto>>>
{
    private readonly IApplicationDbContext _db;

    public TimeSlotsHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result<Guid>> Handle(CreateTimeSlotCommand request, CancellationToken cancellationToken)
    {
        var timeSlot = new TimeSlot(request.StartTime, request.EndTime);
        _db.TimeSlots.Add(timeSlot);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok(timeSlot.Id);
    }

    public async ValueTask<Result> Handle(DeleteTimeSlotCommand request, CancellationToken cancellationToken)
    {
        var timeSlot = await _db.TimeSlots.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (timeSlot is null)
            return Result.Fail($"TimeSlot {request.Id} not found.");

        _db.TimeSlots.Remove(timeSlot);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async ValueTask<Result<IReadOnlyList<TimeSlotDto>>> Handle(GetAllTimeSlotsQuery request, CancellationToken cancellationToken)
    {
        var slots = await _db.TimeSlots
            .OrderBy(t => t.StartTime)
            .Select(t => new TimeSlotDto(t.Id, t.StartTime, t.EndTime))
            .ToListAsync(cancellationToken);

        return Result.Ok((IReadOnlyList<TimeSlotDto>)slots);
    }
}