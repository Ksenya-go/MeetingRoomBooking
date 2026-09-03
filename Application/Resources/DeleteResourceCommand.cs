using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Resources;

public record DeleteResourceCommand(Guid Id) : IRequest<Result>;