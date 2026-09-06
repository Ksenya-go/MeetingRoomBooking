using FluentResults;
using Mediator;

namespace MeetingBooking.Application.Resources.Queries;

public record GetResourceByIdQuery(Guid Id) : IRequest<Result<ResourceDto>>;
