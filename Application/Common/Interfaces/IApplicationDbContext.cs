using MeetingBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace MeetingBooking.Application.Common.Interfaces;

/// <summary>
/// Abstraction over EF Core DbContext that keeps Application
/// independent from Infrastructure
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Resource> Resources { get; }
    DbSet<TimeSlot> TimeSlots { get; }
    DbSet<Booking> Bookings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}