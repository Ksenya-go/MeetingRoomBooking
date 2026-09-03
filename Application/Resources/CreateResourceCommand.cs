using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Resources;

public record CreateResourceCommand(string Name, string? Description, 
    int Capacity) : IRequest<Result<Guid>>;