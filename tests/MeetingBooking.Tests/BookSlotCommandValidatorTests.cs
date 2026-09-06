using FluentValidation.TestHelper;
using MeetingBooking.Application.Bookings.Commands;

namespace MeetingBooking.Tests;

public class BookSlotCommandValidatorTests
{
    private readonly BookSlotCommandValidator _validator = new();

    [Fact]
    public void Rejects_booking_a_date_in_the_past()
    {
        var command = new BookSlotCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Accepts_a_valid_future_booking()
    {
        var command = new BookSlotCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Rejects_empty_resource_id()
    {
        var command = new BookSlotCommand(
            Guid.Empty, Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ResourceId);
    }
}