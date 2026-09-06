using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetingBooking.Application.Resources.Commands;
using MeetingBooking.Application.Resources.Queries;

namespace MeetingBooking.Api.Controllers;

[Authorize]
public class ResourcesController : Controller
{
    private readonly ISender _sender;

    public ResourcesController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetAllResourcesQuery(page, PageSize: 10), cancellationToken);
        return View(result.Value);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateResourceCommand command, 
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors));
            return View(command);
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteResourceCommand(id), cancellationToken);

        if (result.IsFailed)
        {
            TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Message));
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetResourceByIdQuery(id), cancellationToken);
        if (result.IsFailed)
        {
            return NotFound();
        }
      
        var command = new UpdateResourceCommand(result.Value.Id, result.Value.Name, result.Value.Description, result.Value.Capacity);
        return View(command);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateResourceCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors));
            return View(command);
        }
        return RedirectToAction(nameof(Index));
    }


}