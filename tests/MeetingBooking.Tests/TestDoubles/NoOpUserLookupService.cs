using MeetingBooking.Application.Common.Interfaces;

namespace MeetingBooking.Tests.TestDoubles;

public class NoOpUserLookupService : IUserLookupService
{
    public Task<Dictionary<string, string>> GetDisplayNamesAsync(
        IEnumerable<string> userIds, CancellationToken cancellationToken)
        => Task.FromResult(new Dictionary<string, string>());
}