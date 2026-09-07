using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Resources.Commands;
/// <summary>Soft-deletes a meeting room. Admin-only.</summary>
public record DeleteResourceCommand(Guid Id) : IRequest<Result>;