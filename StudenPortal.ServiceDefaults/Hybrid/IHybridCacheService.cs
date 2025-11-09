namespace StudentPortal.ServiceDefaults.Hybrid;

public interface IHybridCacheService
{
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? memoryExpiration = null, TimeSpan? redisExpiration = null);
    Task SetAsync<T>(string key, T value, TimeSpan? memoryExpiration = null, TimeSpan? redisExpiration = null); 
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern); 
}