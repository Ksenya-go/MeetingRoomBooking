using MeetingBooking.Application.Common.Interfaces;
using MeetingBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace MeetingBooking.Infrastructure.Persistence;

public class MeetingBookingDbContext : DbContext, IApplicationDbContext
{
    public MeetingBookingDbContext(DbContextOptions<MeetingBookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeetingBookingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}