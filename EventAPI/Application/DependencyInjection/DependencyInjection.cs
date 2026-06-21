using Application.Contracts;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IEventService, EventService>();
        services.AddSingleton<IBookingService, BookingService>();
    }
}