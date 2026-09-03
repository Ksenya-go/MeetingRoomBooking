using MeetingBooking.Application.Common.Interfaces;

namespace MeetingBooking.Tests.TestDoubles;

public class FakeCurrentUserService : ICurrentUserService
{
    private readonly string _userId;

    public FakeCurrentUserService(string userId) => _userId = userId;

    public string UserId => _userId;
    public bool IsAdmin => false;
}