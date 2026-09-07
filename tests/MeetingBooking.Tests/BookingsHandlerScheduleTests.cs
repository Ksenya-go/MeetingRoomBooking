using FluentAssertions;
using MeetingBooking.Application.Bookings;
using MeetingBooking.Application.Bookings.Queries;
using MeetingBooking.Domain;
using MeetingBooking.Tests.TestDoubles;

namespace MeetingBooking.Tests;

public class BookingsHandlerScheduleTests
{
    [Fact]
    public async Task GetResourceScheduleQuery_marks_booked_slots_correctly()
    {
        await using var db = InMemoryDbContextFactory.Create();

        var resource = new Resource("Room A", null, 5);
        var freeSlot = new TimeSlot(new TimeOnly(9, 0), new TimeOnly(10, 0));
        var bookedSlot = new TimeSlot(new TimeOnly(10, 0), new TimeOnly(11, 0));
        var date = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1));

        db.Resources.Add(resource);
        db.TimeSlots.AddRange(freeSlot, bookedSlot);
        db.Bookings.Add(new Booking(resource.Id, bookedSlot.Id, date, "user-1"));
        await db.SaveChangesAsync();

        var handler = new BookingsHandler(
                        db,
                        new FakeCurrentUserService("user-2"),
                        new NoOpBookingNotifier(),
                        new FakeUserLookupService());
        var result = await handler.Handle(new GetResourceScheduleQuery(resource.Id, date), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().ContainSingle(s => s.TimeSlotId == freeSlot.Id && !s.IsBooked);
        result.Value.Slots.Should().ContainSingle(s => s.TimeSlotId == bookedSlot.Id && s.IsBooked);
    }
}