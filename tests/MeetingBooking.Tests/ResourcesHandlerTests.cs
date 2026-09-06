using FluentAssertions;
using MeetingBooking.Application.Resources;
using MeetingBooking.Application.Resources.Commands;
using MeetingBooking.Domain;
using MeetingBooking.Tests.TestDoubles;

namespace MeetingBooking.Tests;

public class ResourcesHandlerTests
{
    [Fact]
    public async Task CreateResourceCommand_creates_resource_successfully()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var handler = new ResourcesHandler(db);

        var result = await handler.Handle(
            new CreateResourceCommand("Room A", "Big room", 10),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        db.Resources.Should().ContainSingle(r => r.Name == "Room A");
    }

    [Fact]
    public async Task DeleteResourceCommand_fails_when_resource_has_future_bookings()
    {
        await using var db = InMemoryDbContextFactory.Create();

        var resource = new Resource("Room A", null, 5);
        var timeSlot = new TimeSlot(new TimeOnly(9, 0), new TimeOnly(10, 0));
        var futureBooking = new Booking(resource.Id, timeSlot.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)), "user-1");

        db.Resources.Add(resource);
        db.TimeSlots.Add(timeSlot);
        db.Bookings.Add(futureBooking);
        await db.SaveChangesAsync();

        var handler = new ResourcesHandler(db);

        var result = await handler.Handle(new DeleteResourceCommand(resource.Id), CancellationToken.None);

        result.IsFailed.Should().BeTrue(
            "a resource with upcoming bookings must not be removable");
    }

    [Fact]
    public async Task DeleteResourceCommand_succeeds_when_no_future_bookings()
    {
        await using var db = InMemoryDbContextFactory.Create();

        var resource = new Resource("Room B", null, 3);
        db.Resources.Add(resource);
        await db.SaveChangesAsync();

        var handler = new ResourcesHandler(db);

        var result = await handler.Handle(new DeleteResourceCommand(resource.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var updated = await db.Resources.FindAsync(resource.Id);
        updated!.IsActive.Should().BeFalse("delete should soft-deactivate the resource");
    }
}