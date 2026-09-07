using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Resources.Queries;
/// <summary>
/// Fetches a single resource by ID — used for the Edit form
/// </summary>
public record GetResourceByIdQuery(Guid Id) : IRequest<Result<ResourceDto>>;
