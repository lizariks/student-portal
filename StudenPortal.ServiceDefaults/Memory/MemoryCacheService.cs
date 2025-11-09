namespace StudentPortal.ServiceDefaults.Memory;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCacheService> _logger;

        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

        public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public T? Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("MemoryCache key is null or empty");
                return default;
            }

            if (_cache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("MemoryCache hit for key: {Key}", key);
                return value;
            }

            _logger.LogDebug("MemoryCache miss for key: {Key}", key);
            return default;
        }

        public void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("MemoryCache key is null or empty");
                return;
            }

            if (value is null)
            {
                _logger.LogWarning("Attempting to cache null value for key: {Key}", key);
                return;
            }

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? DefaultExpiration,
                SlidingExpiration = slidingExpiration,
                Priority = CacheItemPriority.Normal,
                Size = 1 
            };

            _cache.Set(key, value, options);
            _logger.LogDebug("MemoryCache set for key: {Key} with absolute expiration: {Absolute}, sliding: {Sliding}",
                key, absoluteExpiration ?? DefaultExpiration, slidingExpiration);
        }

        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            _cache.Remove(key);
            _logger.LogDebug("MemoryCache removed key: {Key}", key);
        }

        public void Clear()
        {
            if (_cache is MemoryCache memCache)
            {
                memCache.Compact(1.0); 
                _logger.LogInformation("MemoryCache cleared");
            }
        }
    }