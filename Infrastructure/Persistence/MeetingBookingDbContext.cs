using MeetingBooking.Application.Common.Interfaces;
using MeetingBooking.Domain;
using MeetingBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeetingBooking.Infrastructure.Persistence;

public class MeetingBookingDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
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
        base.OnModelCreating(modelBuilder); 
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeetingBookingDbContext).Assembly);
    }
}