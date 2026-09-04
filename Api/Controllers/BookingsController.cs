using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetingBooking.Application.Bookings;

namespace MeetingBooking.Api.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly ISender _sender;

    public BookingsController(ISender sender)
    {
        _sender = sender;
    }

    // GET /Bookings/Schedule/{resourceId}?date=2026-09-10
    [HttpGet]
    public async Task<IActionResult> Schedule(Guid resourceId, DateOnly? date, CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var result = await _sender.Send(new GetResourceScheduleQuery(resourceId, targetDate), cancellationToken);

        if (result.IsFailed)
        {
            TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Message));
            return RedirectToAction("Index", "Resources");
        }

        return View(result.Value);
    }

    // POST /Bookings/Book
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(Guid resourceId, Guid timeSlotId, DateOnly date, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new BookSlotCommand(resourceId, timeSlotId, date), cancellationToken);

        if (result.IsFailed)
        {
            
            TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Message));
        }
        else
        {
            TempData["Success"] = "Slot booked successfully.";
        }

        return RedirectToAction(nameof(Schedule), new { resourceId, date });
    }
}