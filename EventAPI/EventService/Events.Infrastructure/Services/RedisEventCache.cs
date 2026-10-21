using System.Text.Json;
using Events.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Events.Infrastructure.Services;

public sealed class RedisEventCache(IConnectionMultiplexer connection, ILogger<RedisEventCache> logger) : IEventCache
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        try
        {
            var value = await connection.GetDatabase().StringGetAsync(key).WaitAsync(cancellationToken);
            return value.IsNull ? default : JsonSerializer.Deserialize<T>((string)value!);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Не удалось считать значение по ключу  {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        try
        {
            await connection.GetDatabase().StringSetAsync(key, JsonSerializer.Serialize(value), ttl)
                .WaitAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Не удалось записать значение по ключу {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await connection.GetDatabase().KeyDeleteAsync(key).WaitAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Не удалось удалить значение по ключу {Key}", key);
        }
    }
}
