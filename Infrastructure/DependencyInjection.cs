using MeetingBooking.Application.Common.Interfaces;
using MeetingBooking.Infrastructure.Identity;
using MeetingBooking.Infrastructure.Persistence;
using MeetingBooking.Infrastructure.Realtime;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<MeetingBookingDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<MeetingBookingDbContext>());

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IBookingNotifier, SignalRBookingNotifier>();
        services.AddScoped<IUserLookupService, UserLookupService>();

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<MeetingBookingDbContext>()
            .AddDefaultTokenProviders();

        services.AddSignalR().AddAzureSignalR();

        return services;
    }
}