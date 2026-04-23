using FlashSales.Application.Cache;
using FlashSales.Application.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Text.Json;

namespace FlashSales.Infrastructure.Cache
{
    internal sealed class CacheService(IDistributedCache cache, ILogger<CacheService> logger) : ICacheService
    {
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            byte[]? bytes = await cache.GetAsync(key, cancellationToken);

            if (bytes is null)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("[CACHE MISS] for key: {Key}", key);
                }

                return default;
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("[CACHE HIT] for key: {Key}", key);
            }

            return Deserialize<T>(bytes);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => cache.RemoveAsync(key, cancellationToken);

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
        {
            byte[] bytes = Serialize(value);

            var cacheExpiration = CacheOptions.Create(expiration);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Setting cache for key: {Key} with expiration: {Expiration}",
                    key,
                    cacheExpiration.AbsoluteExpirationRelativeToNow);
            }

            return cache.SetAsync(key, bytes, cacheExpiration, cancellationToken);
        }

        private static T Deserialize<T>(byte[] bytes)
            => JsonSerializer.Deserialize<T>(bytes, JsonSerializerOptionsExtensions.GetDefault())!;

        private static byte[] Serialize<T>(T value)
        {
            var buffer = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(buffer);
            JsonSerializer.Serialize(writer, value, JsonSerializerOptionsExtensions.GetDefault());
            return buffer.WrittenSpan.ToArray();
        }

        public Task SetAsync<T>(string key, T value, int? expiration = null, CancellationToken cancellationToken = default) where T : class
        {
            return SetAsync(key, value, expiration.HasValue ? TimeSpan.FromMinutes(expiration.Value) : null, cancellationToken);
        }
    }
}