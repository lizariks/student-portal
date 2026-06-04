namespace StudentPortal.ServiceDefaults.Metrics;

using System.Diagnostics.Metrics;


public class CacheMetrics : IDisposable
{
    private readonly Meter _meter; 
    
    public Counter<long> CacheHitsCounter { get; }
    public Counter<long> CacheMissesCounter { get; }
    public Counter<long> CacheInvalidationsCounter { get; }
    
    public Histogram<double> CacheLatency { get; }
    
    private readonly ObservableGauge<int> _cacheSizeGauge;
    
    public CacheMetrics()
    {
        _meter = new Meter("StudentPortal.AggregatorService.Cache", "1.0");
        
        CacheHitsCounter = _meter.CreateCounter<long>(
            "app_cache_hits_total", 
            "operations", 
            "Total number of cache hits.");

        CacheMissesCounter = _meter.CreateCounter<long>(
            "app_cache_misses_total", 
            "operations", 
            "Total number of cache misses.");

        CacheInvalidationsCounter = _meter.CreateCounter<long>(
            "app_cache_invalidations_total",
            "events",
            "Total number of explicit cache invalidation events.");

        CacheLatency = _meter.CreateHistogram<double>(
            "app_cache_latency_seconds",
            "seconds",
            "Latency of cache operations (Get/Set).");
      
    }

    public void Dispose()
    {
        _meter.Dispose();
    }
}