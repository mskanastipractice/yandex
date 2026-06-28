using Application.Contracts;
using Infrastructure.BackgroundServices;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHostedService<BookingBackgroundService>();
        services.AddSingleton<IEventRepository, EventRepository>();
        services.AddSingleton<IBookingRepository, BookingRepository>();
    }
}