namespace StudentPortal.UnitTests.Integration;

using StudentPortal.ServiceDefaults.Hybrid;


/// <summary>
/// Test double for IHybridCacheService that bypasses all caching
/// and always invokes the factory delegate directly.
/// This ensures integration tests always hit the real database.
/// </summary>
public class PassthroughHybridCacheService : IHybridCacheService
{
    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T?>> factory,
        TimeSpan? memoryExpiration = null,
        TimeSpan? redisExpiration = null)
    {
        // Always invoke factory — no cache, always goes to DB
        return await factory();
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? memoryExpiration = null,
        TimeSpan? redisExpiration = null)
    {
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        return Task.CompletedTask;
    }
}