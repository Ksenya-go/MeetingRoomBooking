using Microsoft.AspNetCore.Http;
using MeetingBooking.Application.Common.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
namespace MeetingBooking.Infrastructure.Identity;

/// <summary>
/// Exposes the current user's ID and Admin status to the Application layer
/// without direct dependency on HttpContext.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId =>
    _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
    ?? throw new InvalidOperationException("No authenticated user in context.");

    public bool IsAdmin =>
        _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
}