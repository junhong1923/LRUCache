using System;
using System.Collections.Generic;

namespace LRUCache.Models
{
    /// <summary>
    /// 可序列化的快照類別，用於持久化
    /// </summary>
    public class SerializableSnapshot<TKey, TValue> where TKey : notnull
    {
        public Dictionary<TKey, TValue> CacheData { get; set; } = new Dictionary<TKey, TValue>();
        public List<TKey> UsageOrder { get; set; } = new List<TKey>();
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 持久化數據結構，包含完整的緩存狀態
    /// </summary>
    public class CachePersistData<TKey, TValue> where TKey : notnull
    {
        public int Capacity { get; set; }
        public Dictionary<TKey, TValue> CacheData { get; set; } = new Dictionary<TKey, TValue>();
        public List<TKey> UsageOrder { get; set; } = new List<TKey>();
        public List<SerializableSnapshot<TKey, TValue>> History { get; set; } = new List<SerializableSnapshot<TKey, TValue>>();
    }
}
