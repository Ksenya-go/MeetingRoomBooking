using FluentValidation;

namespace MeetingBooking.Application.Bookings.Commands;

public class BookSlotCommandValidator : AbstractValidator<BookSlotCommand>
{
    public BookSlotCommandValidator()
    {
        RuleFor(x => x.ResourceId).NotEmpty();
        RuleFor(x => x.TimeSlotId).NotEmpty();
        RuleFor(x => x.Date).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Cannot book a slot in the past.");
    }
}