using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Resources.Queries;

public record GetAllResourcesQuery : IRequest<Result<IReadOnlyList<ResourceDto>>>;