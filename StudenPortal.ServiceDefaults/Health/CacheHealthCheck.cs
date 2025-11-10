namespace StudentPortal.ServiceDefaults.Health;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

    public class CacheHealthCheck : IHealthCheck
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConnectionMultiplexer _redisMultiplexer;
        private readonly TimeSpan _timeout;

        public CacheHealthCheck(
            IMemoryCache memoryCache,
            IConnectionMultiplexer redisMultiplexer,
            TimeSpan? timeout = null)
        {
            _memoryCache = memoryCache;
            _redisMultiplexer = redisMultiplexer;
            _timeout = timeout ?? TimeSpan.FromSeconds(3);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, object>();

            CollectMemoryCacheStats(data);

            var redisHealthy = await CheckRedisHealthAsync(data, cancellationToken);

            if (!redisHealthy)
            {
                return HealthCheckResult.Degraded(
                    "Redis cache unavailable. Application running with memory cache only.",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                "L1 (Memory) and L2 (Redis) cache services are operational.",
                data);
        }

        private void CollectMemoryCacheStats(Dictionary<string, object> data)
        {
            var stats = _memoryCache.GetCurrentStatistics();
            if (stats is not null)
            {
                data["L1_MemoryCache_Hits"] = stats.TotalHits;
                data["L1_MemoryCache_Misses"] = stats.TotalMisses;
                data["L1_MemoryCache_Entries"] = stats.CurrentEntryCount;
                data["L1_MemoryCache_SizeBytes"] = stats.CurrentEstimatedSize ?? 0;

                if (stats.TotalHits + stats.TotalMisses > 0)
                {
                    var hitRate = (double)stats.TotalHits / (stats.TotalHits + stats.TotalMisses) * 100;
                    data["L1_MemoryCache_HitRate"] = $"{hitRate:F2}%";
                }
            }
        }

        private async Task<bool> CheckRedisHealthAsync(
            Dictionary<string, object> data,
            CancellationToken cancellationToken)
        {
            try
            {
                var db = _redisMultiplexer.GetDatabase();

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(_timeout);

                var pingTime = await db.PingAsync();

                data["L2_Redis_Status"] = "Connected";
                data["L2_Redis_PingMs"] = pingTime.TotalMilliseconds;
                data["L2_Redis_Endpoints"] = string.Join(", ",
                    _redisMultiplexer.GetEndPoints().Select(e => e.ToString()));

                return true;
            }
            catch (RedisConnectionException ex)
            {
                data["L2_Redis_Status"] = "Disconnected";
                data["L2_Redis_Error"] = ex.Message;
                return false;
            }
            catch (OperationCanceledException)
            {
                data["L2_Redis_Status"] = "Timeout";
                data["L2_Redis_Error"] = $"Connection timeout after {_timeout.TotalSeconds}s";
                return false;
            }
            catch (Exception ex)
            {
                data["L2_Redis_Status"] = "Error";
                data["L2_Redis_Error"] = ex.Message;
                return false;
            }
        }
    }