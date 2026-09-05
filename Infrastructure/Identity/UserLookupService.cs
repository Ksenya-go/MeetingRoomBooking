using MeetingBooking.Application.Common.Interfaces;
using MeetingBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeetingBooking.Infrastructure.Identity;

public class UserLookupService : IUserLookupService
{
    private readonly MeetingBookingDbContext _db;

    public UserLookupService(MeetingBookingDbContext db)
    {
        _db = db;
    }

    public async Task<Dictionary<string, string>> GetDisplayNamesAsync(
        IEnumerable<string> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.Distinct().ToList();

        var users = await _db.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName, u.Email })
            .ToListAsync(cancellationToken);

        return users.ToDictionary(
            u => u.Id,
            u => string.IsNullOrWhiteSpace(u.FullName)
                ? (u.Email ?? u.Id)
                : $"{u.FullName} ({u.Email})");
    }
}