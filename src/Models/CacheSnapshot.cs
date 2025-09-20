namespace LRUCache.Models
{
    /// <summary>
    /// CacheSnapshot for storing the state of the cache at a specific point in time
    /// </summary>
    public class CacheSnapshot<TKey, TValue> where TKey : notnull
    {
        public Dictionary<TKey, TValue> CacheData { get; set; }
        public List<TKey> UsageOrder { get; set; }
        public DateTime Timestamp { get; set; }
        
        public CacheSnapshot(Dictionary<TKey, TValue> cacheData, List<TKey> usageOrder)
        {
            CacheData = new Dictionary<TKey, TValue>(cacheData);
            UsageOrder = new List<TKey>(usageOrder);
            Timestamp = DateTime.Now;
        }
    }
}
