using System;
using System.Collections.Generic;

namespace LRUCache.Models
{
    /// <summary>
    /// 快照類別，用於存儲緩存狀態以支援回滾功能
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
