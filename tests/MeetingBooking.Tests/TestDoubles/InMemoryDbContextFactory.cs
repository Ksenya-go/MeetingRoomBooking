using Microsoft.EntityFrameworkCore;
using MeetingBooking.Infrastructure.Persistence;

namespace MeetingBooking.Tests.TestDoubles;

public static class InMemoryDbContextFactory
{
    public static MeetingBookingDbContext Create()
    {
        var options = new DbContextOptionsBuilder<MeetingBookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MeetingBookingDbContext(options);
    }
}