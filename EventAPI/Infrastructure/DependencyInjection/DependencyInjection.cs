using Application.Contracts;
using Application.Contracts.Infrastructure;
using Application.Services;
using Infrastructure.BackgroundServices;
using Infrastructure.Queue;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHostedService<BookingBackgroundService>();
        services.AddSingleton<IBookingTaskQueue, InMemoryBookingTaskQueue>();
    }
}