using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetingBooking.Application.Resources;
using MeetingBooking.Application.Resources.Commands;

namespace MeetingBooking.Api.Controllers;

[Authorize]
public class ResourcesController : Controller
{
    private readonly ISender _sender;

    public ResourcesController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAllResourcesQuery(), cancellationToken);
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
        var result = await _sender.Send(new GetAllResourcesQuery(), cancellationToken);
        var resource = result.Value?.FirstOrDefault(r => r.Id == id);

        if (resource is null)
            return NotFound();

        var command = new UpdateResourceCommand(resource.Id, resource.Name, resource.Description, resource.Capacity);
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