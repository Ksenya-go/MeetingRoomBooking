using FluentAssertions;
using MeetingBooking.Application.Bookings;
using MeetingBooking.Domain;
using MeetingBooking.Infrastructure.Persistence;
using MeetingBooking.Tests.TestDoubles;
using Microsoft.EntityFrameworkCore;

namespace MeetingBooking.Tests;

public class BookingConcurrencyTests : IAsyncLifetime
{

    private static readonly string ConnectionString =
    Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING")
    ?? throw new InvalidOperationException(
        "Set the TEST_DB_CONNECTION_STRING environment variable before running tests. " +
        "See README.md for instructions.");

    private Guid _resourceId;
    private Guid _timeSlotId;
    private readonly DateOnly _date = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1));

    public async Task InitializeAsync()
    {
        await using var db = CreateContext();

        var resource = new Resource("Concurrency Test Room", null, 4);
        var timeSlot = new TimeSlot(new TimeOnly(9, 0), new TimeOnly(10, 0));

        db.Resources.Add(resource);
        db.TimeSlots.Add(timeSlot);
        await db.SaveChangesAsync();

        _resourceId = resource.Id;
        _timeSlotId = timeSlot.Id;
    }

    public async Task DisposeAsync()
    {
        await using var db = CreateContext();
        db.Bookings.RemoveRange(db.Bookings.Where(b => b.ResourceId == _resourceId));
        db.TimeSlots.RemoveRange(db.TimeSlots.Where(t => t.Id == _timeSlotId));
        db.Resources.RemoveRange(db.Resources.Where(r => r.Id == _resourceId));
        await db.SaveChangesAsync();
    }

    private static MeetingBookingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MeetingBookingDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new MeetingBookingDbContext(options);
    }

    [Fact]
    public async Task Concurrent_booking_requests_for_same_slot_result_in_exactly_one_success()
    {
        const int concurrentRequests = 10;

        var tasks = Enumerable.Range(0, concurrentRequests).Select(async i =>
        {
            await using var db = CreateContext();
            var handler = new BookingsHandler(
                db,
                new FakeCurrentUserService($"user-{i}"),
                new NoOpBookingNotifier());

            var command = new BookSlotCommand(_resourceId, _timeSlotId, _date);
            return await handler.Handle(command, CancellationToken.None);
        });

        var results = await Task.WhenAll(tasks);

        results.Count(r => r.IsSuccess).Should().Be(1,
            "exactly one concurrent request should win the race for the same slot");
        results.Count(r => r.IsFailed).Should().Be(concurrentRequests - 1,
            "all other requests should receive a clear conflict result, not an exception");

        await using var verifyDb = CreateContext();
        var bookingsCount = await verifyDb.Bookings
            .CountAsync(b => b.ResourceId == _resourceId && b.TimeSlotId == _timeSlotId && b.Date == _date);

        bookingsCount.Should().Be(1, "the database must contain exactly one booking for this slot");
    }
}