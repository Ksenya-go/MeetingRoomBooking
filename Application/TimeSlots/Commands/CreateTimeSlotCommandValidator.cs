using FluentValidation;

namespace MeetingBooking.Application.TimeSlots.Commands;

public class CreateTimeSlotCommandValidator : AbstractValidator<CreateTimeSlotCommand>
{
    public static readonly TimeOnly EarliestStart = new(9, 0);
    public static readonly TimeOnly LatestEnd = new(22, 0);

    public CreateTimeSlotCommandValidator()
    {
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");

        RuleFor(x => x.StartTime)
            .GreaterThanOrEqualTo(EarliestStart)
            .WithMessage($"Time slots cannot start before {EarliestStart:HH:mm}.");

        RuleFor(x => x.EndTime)
            .LessThanOrEqualTo(LatestEnd)
            .WithMessage($"Time slots cannot end after {LatestEnd:HH:mm}.");
    }
}