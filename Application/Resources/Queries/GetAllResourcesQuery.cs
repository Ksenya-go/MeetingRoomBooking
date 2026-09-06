using FluentResults;
using Mediator;
using X.PagedList;

namespace MeetingBooking.Application.Resources.Queries;

public record GetAllResourcesQuery(int PageNumber, int PageSize) : IRequest<Result<IPagedList<ResourceDto>>>;