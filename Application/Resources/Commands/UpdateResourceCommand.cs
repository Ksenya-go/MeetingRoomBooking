using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Resources.Commands;
/// <summary>Updates a meeting room's details. Admin-only.</summary>
public record UpdateResourceCommand(Guid Id, string Name, string? Description, 
    int Capacity) : IRequest<Result>;