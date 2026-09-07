using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Resources.Commands;
/// <summary>Creates a new meeting room. Admin-only.</summary>
public record CreateResourceCommand(string Name, string? Description, 
    int Capacity) : IRequest<Result<Guid>>;