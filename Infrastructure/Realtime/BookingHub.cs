using Microsoft.AspNetCore.SignalR;

namespace MeetingBooking.Infrastructure.Realtime;
/// <summary>
/// SignalR hub for real-time booking updates, scoped to the currently viewed Resource.
/// </summary>
public class BookingHub : Hub
{
    public async Task JoinResourceGroup(string resourceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(resourceId));
    }

    public async Task LeaveResourceGroup(string resourceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(resourceId));
    }

    public static string GroupName(string resourceId) => $"resource-{resourceId}";
}