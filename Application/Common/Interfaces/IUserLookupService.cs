namespace MeetingBooking.Application.Common.Interfaces;

public interface IUserLookupService
{
    Task<Dictionary<string, string>> GetDisplayNamesAsync(
        IEnumerable<string> userIds, CancellationToken cancellationToken);
}