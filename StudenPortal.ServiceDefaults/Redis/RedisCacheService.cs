namespace StudentPortal.ServiceDefaults.Redis;

using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _multiplexer;
        private readonly IDatabase _db;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);
        private readonly AsyncRetryPolicy _retryPolicy;
        public IConnectionMultiplexer GetMultiplexer() => _multiplexer;

        public RedisCacheService(
            IConnectionMultiplexer multiplexer,
            ILogger<RedisCacheService> logger)
        {
            _multiplexer = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = _multiplexer.GetDatabase();

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            _retryPolicy = Policy
                .Handle<RedisException>()
                .Or<RedisTimeoutException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(100 * retryAttempt),
                    onRetry: (exception, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception,
                            "Retry {RetryCount} for Redis operation due to {ExceptionType}. Delaying for {Delay}ms.",
                            retryCount, exception.GetType().Name, timespan.TotalMilliseconds);
                    });

            _logger.LogInformation("RedisCacheService initialized with Polly retry policy.");
        }

        public async Task<T?> GetDataAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Cache key is null or empty");
                return default;
            }

            try 
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    var data = await _db.StringGetAsync(key);
                    if (data.IsNullOrEmpty)
                    {
                        _logger.LogInformation("L2 Cache MISS for key: {Key}", key);
                        return default(T);
                    }
                    return JsonSerializer.Deserialize<T>(data!, _jsonOptions);
                });
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Final Redis operation failure for key {Key} after retries. Returning default.", key);
                return default; 
            }
        }
        public async Task SetDataAsync<T>(string key, T data, TimeSpan? expiration = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Cache key is null or empty");
                return;
            }
            if (data is null)
            {
                _logger.LogWarning("Attempting to cache null data for key: {Key}", key);
                return;
            }

            await _retryPolicy.ExecuteAsync(async () =>
            {
                var serializedData = JsonSerializer.Serialize(data, _jsonOptions);
                await _db.StringSetAsync(key, serializedData, expiration ?? DefaultExpiration);
            });

            _logger.LogDebug("Data cached successfully for key: {Key} with expiration: {Expiration}",
                key, expiration ?? DefaultExpiration);
        }

        public async Task RemoveDataAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            await _retryPolicy.ExecuteAsync(() => _db.KeyDeleteAsync(key));
            _logger.LogDebug("Cache key removed: {Key}", key);
        }

        public async Task AddToSetAsync(string setKey, string value)
        {
            if (string.IsNullOrWhiteSpace(setKey) || string.IsNullOrWhiteSpace(value)) return;

            await _retryPolicy.ExecuteAsync(() => _db.SetAddAsync(setKey, value));
        }

        public async Task<IEnumerable<string>> GetSetMembersAsync(string setKey)
        {
            if (string.IsNullOrWhiteSpace(setKey)) return Enumerable.Empty<string>();

            var members = await _retryPolicy.ExecuteAsync(() => _db.SetMembersAsync(setKey));
            return members.Select(m => m.ToString());
        }

        public async Task ClearSetAsync(string setKey)
        {
            if (string.IsNullOrWhiteSpace(setKey)) return;

            await _retryPolicy.ExecuteAsync(() => _db.KeyDeleteAsync(setKey));
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return;

            try
            {
                var endpoints = _multiplexer.GetEndPoints();
                var deletedCount = 0;

                foreach (var endpoint in endpoints)
                {
                    var server = _multiplexer.GetServer(endpoint);
                    await foreach (var key in server.KeysAsync(pattern: pattern, pageSize: 250))
                    {
                        await _retryPolicy.ExecuteAsync(() => _db.KeyDeleteAsync(key));
                        deletedCount++;
                    }
                }

                _logger.LogInformation("Removed {Count} keys matching pattern: {Pattern}",
                    deletedCount, pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove keys by pattern: {Pattern}", pattern);
                throw;
            }
        }
    }