namespace MeetingBooking.Application.Common.Interfaces;


public interface ICurrentUserService
{
    string UserId { get; }
    bool IsAdmin { get; }
}