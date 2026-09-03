using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Resources;

public record UpdateResourceCommand(Guid Id, string Name, string? Description, 
    int Capacity) : IRequest<Result>;