using Microsoft.AspNetCore.SignalR;
using MeetingBooking.Application.Common.Interfaces;

namespace MeetingBooking.Infrastructure.Realtime;

/// <summary>
/// Sends real-time slot-status notifications via Azure SignalR Service
/// after a successful booking. 
/// </summary>

public class SignalRBookingNotifier : IBookingNotifier
{
    private readonly IHubContext<BookingHub> _hubContext;

    public SignalRBookingNotifier(IHubContext<BookingHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifySlotBookedAsync(Guid resourceId, Guid timeSlotId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var group = BookingHub.GroupName(resourceId.ToString());

        await _hubContext.Clients.Group(group).SendAsync(
            "SlotBooked",
            new { timeSlotId, date = date.ToString("yyyy-MM-dd") },
            cancellationToken);
    }
}