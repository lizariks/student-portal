namespace StudentPortal.ServiceDefaults.Hybrid;

using StudentPortal.ServiceDefaults.Memory;
using StudentPortal.ServiceDefaults.Redis;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;


    public class HybridCacheService : IHybridCacheService, IDisposable
    {
        private readonly IMemoryCacheService _memoryCache;
        private readonly IRedisCacheService _redisCache;
        private readonly ILogger<HybridCacheService> _logger;
        private readonly ISubscriber _subscriber;
        private const string InvalidationChannel = "cache-invalidation-channel";
        private const string ClearAllMessage = "__CLEAR_ALL__";

        private bool _disposed;

        public HybridCacheService(
            IMemoryCacheService memoryCache,
            IRedisCacheService redisCache,
            IConnectionMultiplexer redisMultiplexer,
            ILogger<HybridCacheService> logger)
        {
            _memoryCache = memoryCache;
            _redisCache = redisCache;
            _logger = logger;

            _subscriber = redisMultiplexer.GetSubscriber();

            _subscriber.Subscribe(RedisChannel.Literal(InvalidationChannel), (channel, message) =>
            {
                var messageStr = message.ToString();

                if (messageStr == ClearAllMessage)
                {
                    _logger.LogWarning("Received a full L1 cache clear signal. Clearing local L1 cache.");
                    _memoryCache.Clear();
                }
                else
                {
                    _logger.LogInformation("Received cache invalidation for key: {Key}", messageStr);
                    _memoryCache.Remove(messageStr);
                }
            });
        }

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? memoryExpiration = null, TimeSpan? redisExpiration = null)
        {
            var memoryData = _memoryCache.Get<T>(key);
            if (memoryData is not null)
            {
                _logger.LogInformation("L1 Cache HIT for key: {Key}", key);
                return memoryData;
            }

            var redisData = await _redisCache.GetDataAsync<T>(key);
            if (redisData is not null)
            {
                _logger.LogInformation("L2 Cache HIT for key: {Key}", key);
                _memoryCache.Set(key, redisData, memoryExpiration ?? TimeSpan.FromMinutes(1));
                return redisData;
            }

            var dbData = await factory();

            if (dbData is not null)
            {
                _logger.LogInformation("DB HIT for key: {Key}. Caching now.", key);
                await _redisCache.SetDataAsync(key, dbData, redisExpiration ?? TimeSpan.FromMinutes(5));
                _memoryCache.Set(key, dbData, memoryExpiration ?? TimeSpan.FromMinutes(1));
            }

            return dbData;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? memoryExpiration = null, TimeSpan? redisExpiration = null)
        {
            _memoryCache.Set(key, value, memoryExpiration);
            await _redisCache.SetDataAsync(key, value, redisExpiration);
            await _subscriber.PublishAsync(RedisChannel.Literal(InvalidationChannel), key);

            _logger.LogDebug("HybridCache SET for key: {Key} and published invalidation.", key);
        }

        public async Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            await _redisCache.RemoveDataAsync(key);
            await _subscriber.PublishAsync(RedisChannel.Literal(InvalidationChannel), key);

            _logger.LogDebug("HybridCache REMOVED for key: {Key} and published invalidation.", key);
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            await _redisCache.RemoveByPatternAsync(pattern);
            await _subscriber.PublishAsync(RedisChannel.Literal(InvalidationChannel), ClearAllMessage);

            _logger.LogInformation("HybridCache REMOVED by pattern '{Pattern}' and published a full L1 clear signal.", pattern);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _subscriber?.Unsubscribe(RedisChannel.Literal(InvalidationChannel));
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }