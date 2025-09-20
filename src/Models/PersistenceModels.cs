namespace LRUCache.Models
{
    /// <summary>
    /// Persistent Cache Data
    /// </summary>
    public class CachePersistData<TKey, TValue> where TKey : notnull
    {
        public int Capacity { get; set; }
        public Dictionary<TKey, TValue> CacheData { get; set; } = new Dictionary<TKey, TValue>();
        public List<TKey> UsageOrder { get; set; } = new List<TKey>();
        public List<SerializableSnapshot<TKey, TValue>> History { get; set; } = new List<SerializableSnapshot<TKey, TValue>>();
    }

    /// <summary>
    /// Serializable Snapshot
    /// </summary>
    public class SerializableSnapshot<TKey, TValue> where TKey : notnull
    {
        public Dictionary<TKey, TValue> CacheData { get; set; } = new Dictionary<TKey, TValue>();
        public List<TKey> UsageOrder { get; set; } = new List<TKey>();
        public DateTime Timestamp { get; set; }
    }
}
