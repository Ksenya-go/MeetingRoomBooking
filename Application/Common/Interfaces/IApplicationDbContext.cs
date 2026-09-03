using MeetingBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace MeetingBooking.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Resource> Resources { get; }
    DbSet<TimeSlot> TimeSlots { get; }
    DbSet<Booking> Bookings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}