using FluentResults;
using Mediator;
using MeetingBooking.Application.Common.Interfaces;
using MeetingBooking.Application.Resources.Commands;
using MeetingBooking.Application.Resources.Queries;
using MeetingBooking.Domain;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace MeetingBooking.Application.Resources;

public class ResourcesHandler :
    IRequestHandler<CreateResourceCommand, Result<Guid>>,
    IRequestHandler<UpdateResourceCommand, Result>,
    IRequestHandler<DeleteResourceCommand, Result>,
    IRequestHandler<GetAllResourcesQuery, Result<IPagedList<ResourceDto>>>,
    IRequestHandler<GetResourceByIdQuery, Result<ResourceDto>>
{
    private readonly IApplicationDbContext _db;

    public ResourcesHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result<Guid>> Handle(CreateResourceCommand request, CancellationToken 
        cancellationToken)
    {
        var resource = new Resource(request.Name, request.Description, request.Capacity);
        _db.Resources.Add(resource);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok(resource.Id);
    }

    public async ValueTask<Result> Handle(UpdateResourceCommand request, CancellationToken 
        cancellationToken)
    {
        var resource = await _db.Resources.FirstOrDefaultAsync(r => r.Id == request.Id, 
            cancellationToken);
        if (resource is null)
        {
            return Result.Fail($"Resource {request.Id} not found.");
        }

        resource.Update(request.Name, request.Description, request.Capacity);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async ValueTask<Result> Handle(DeleteResourceCommand request, CancellationToken 
        cancellationToken)
    {
        var resource = await _db.Resources.FirstOrDefaultAsync(r => r.Id == request.Id, 
            cancellationToken);
        if (resource is null)
        {
            return Result.Fail($"Resource {request.Id} not found.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var hasFutureBookings = await _db.Bookings
            .AnyAsync(b => b.ResourceId == request.Id && b.Date >= today, cancellationToken);

        if (hasFutureBookings)
        {
            return Result.Fail(
                "Cannot remove this resource: it has upcoming bookings. Cancel or wait for " +
                "them to pass first.");
        }

        resource.Deactivate();
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async ValueTask<Result<IPagedList<ResourceDto>>> Handle(GetAllResourcesQuery request, 
        CancellationToken cancellationToken)
    {
        var baseQuery = _db.Resources
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .Select(r => new ResourceDto(r.Id, r.Name, r.Description, r.Capacity, r.IsActive));

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var resources = await baseQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var pagedList = new StaticPagedList<ResourceDto>(resources, request.PageNumber, 
            request.PageSize, totalCount);

        return Result.Ok((IPagedList<ResourceDto>)pagedList);
    }

    public async ValueTask<Result<ResourceDto>> Handle(GetResourceByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var resource = await _db.Resources.FirstOrDefaultAsync(r => r.Id == request.Id, 
            cancellationToken);
        if (resource is null)
        {
            return Result.Fail($"Resource {request.Id} not found.");
        }
  
        return Result.Ok(new ResourceDto(resource.Id, resource.Name, resource.Description, 
            resource.Capacity, resource.IsActive));
    }

}