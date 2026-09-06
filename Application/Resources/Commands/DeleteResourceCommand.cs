using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Resources.Commands;

public record DeleteResourceCommand(Guid Id) : IRequest<Result>;