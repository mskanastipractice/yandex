using Common.Contracts;
using Events.Application.Interfaces;
using Events.Application.Settings;
using Events.Infrastructure.BackgroundServices;
using Events.Infrastructure.DataAccess;
using Events.Infrastructure.HealthChecker;
using Events.Infrastructure.Repositories;
using Events.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Не найдена строка подключения к БД.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        var redisConnection = configuration["Redis:ConnectionString"] ?? "localhost:6379";
        var redisOptions = ConfigurationOptions.Parse(redisConnection);
        redisOptions.AbortOnConnectFail = false;
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));
        services.AddSingleton<IEventCache, RedisEventCache>();
        var cacheSettings = configuration.GetSection("Redis").Get<EventCacheSettings>() ?? new EventCacheSettings();
        if (cacheSettings.EventTtlSeconds <= 0 || cacheSettings.TopTtlSeconds <= 0)
            throw new InvalidOperationException("Не указано TTL для Redis.");
        services.AddSingleton(cacheSettings);

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IDatabaseHealthChecker, DatabaseHealthChecker>();
        
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        services.AddSingleton(BindKafkaSettings(configuration));
        services.AddHostedService<KafkaTopicInitializer>();
        services.AddHostedService<BookingConfirmedConsumer>();
    }

    private static KafkaSettings BindKafkaSettings(IConfiguration configuration)
    {
        var settings = configuration.GetSection(KafkaSettings.SectionName).Get<KafkaSettings>()
                       ?? throw new InvalidOperationException($"Секция конфигурации '{KafkaSettings.SectionName}' не найдена.");

        if (string.IsNullOrWhiteSpace(settings.BootstrapServers))
        {
            throw new InvalidOperationException("Не найден адрес брокера Kafka в конфигурации [секция 'Kafka:BootstrapServers'].");
        }

        if (string.IsNullOrWhiteSpace(settings.ConsumerGroup))
        {
            throw new InvalidOperationException("Не найден идентификатор группы потребителей Kafka в конфигурации [секция 'Kafka:ConsumerGroup'].");
        }

        return settings;
    }
}