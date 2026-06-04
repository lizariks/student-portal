namespace StudentPortal.ServiceDefaults.Memory;

public interface IMemoryCacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
    void Remove(string key);
    void Clear(); 
}