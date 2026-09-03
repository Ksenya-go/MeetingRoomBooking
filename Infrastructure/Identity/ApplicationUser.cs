using Microsoft.AspNetCore.Identity;

namespace MeetingBooking.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}