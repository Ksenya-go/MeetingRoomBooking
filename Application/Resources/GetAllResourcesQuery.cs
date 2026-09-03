using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Resources;

public record GetAllResourcesQuery : IRequest<Result<IReadOnlyList<ResourceDto>>>;