using MeetingBooking.Application.Common.Interfaces;

namespace MeetingBooking.Tests.TestDoubles;

public class FakeUserLookupService : IUserLookupService
{
    public Task<Dictionary<string, string>> GetDisplayNamesAsync(
        IEnumerable<string> userIds, CancellationToken cancellationToken)
    {
        var result = userIds.Distinct().ToDictionary(id => id, id => id);
        return Task.FromResult(result);
    }
}