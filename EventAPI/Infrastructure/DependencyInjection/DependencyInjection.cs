using Application.Contracts;
using Infrastructure.Auth;
using Infrastructure.BackgroundServices;
using Infrastructure.DataAccess;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Connection string 'Default' not found.");
		
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddHostedService<BookingBackgroundService>();
        
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();
    }

    public static void AddDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}