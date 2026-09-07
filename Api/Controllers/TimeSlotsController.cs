using Mediator;
using MeetingBooking.Application.Queries.TimeSlots;
using MeetingBooking.Application.TimeSlots.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace MeetingBooking.Api.Controllers;

[Authorize(Roles = "Admin")]
public class TimeSlotsController : Controller
{
    private readonly ISender _sender;

    public TimeSlotsController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetAllTimeSlotsQuery(page, PageSize: 7), cancellationToken);
        return View(result.Value);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTimeSlotCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors));
            return View(command);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteTimeSlotCommand(id), cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}