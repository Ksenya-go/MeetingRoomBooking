namespace MeetingBooking.Application.Common.Interfaces;
/// <summary>
/// Resolves user IDs to display names for showing "who booked this slot"
/// </summary>
public interface IUserLookupService
{
    Task<Dictionary<string, string>> GetDisplayNamesAsync(
        IEnumerable<string> userIds, CancellationToken cancellationToken);
}