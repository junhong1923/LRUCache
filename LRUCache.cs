using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;

// Snapshot class to store cache state for rollback
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

// Serializable snapshot for persistence
public class SerializableSnapshot<TKey, TValue> where TKey : notnull
{
    public Dictionary<TKey, TValue> CacheData { get; set; } = new Dictionary<TKey, TValue>();
    public List<TKey> UsageOrder { get; set; } = new List<TKey>();
    public DateTime Timestamp { get; set; }
}

// Persistence data structure
public class CachePersistData<TKey, TValue> where TKey : notnull
{
    public int Capacity { get; set; }
    public Dictionary<TKey, TValue> CacheData { get; set; } = new Dictionary<TKey, TValue>();
    public List<TKey> UsageOrder { get; set; } = new List<TKey>();
    public List<SerializableSnapshot<TKey, TValue>> History { get; set; } = new List<SerializableSnapshot<TKey, TValue>>();
}

// Simple LRU Cache implementation using Dictionary + List with History and Rollback
public class LRUCache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, TValue> _cache;
    private readonly List<TKey> _usageOrder; // Most recently used at the end
    private readonly List<CacheSnapshot<TKey, TValue>> _history; // History for rollback
    
    public LRUCache(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(capacity));
        
        _capacity = capacity;
        _cache = new Dictionary<TKey, TValue>();
        _usageOrder = new List<TKey>();
        _history = new List<CacheSnapshot<TKey, TValue>>();
        
        // Create initial snapshot
        CreateSnapshot();
    }

    public int Count => _cache.Count;
    public int Capacity => _capacity;
    public int HistoryCount => _history.Count;

    // Create a snapshot of current state
    private void CreateSnapshot()
    {
        var snapshot = new CacheSnapshot<TKey, TValue>(_cache, _usageOrder);
        _history.Add(snapshot);
    }

    // Task 1.2: Get method - retrieve value and update usage order
    public TValue? Get(TKey key)
    {
        if (!_cache.TryGetValue(key, out TValue? value))
        {
            return default(TValue);
        }

        // Update the usage order
        _usageOrder.Remove(key);
        _usageOrder.Add(key); // Move to end (most recently used)
        
        // Create snapshot after state change
        CreateSnapshot();

        return value;
    }

    // Task 1.3: Put method - add/update value and manage capacity
    public void Put(TKey key, TValue value)
    {
        if (_cache.ContainsKey(key))
        {
            // Update existing key
            _cache[key] = value;
            _usageOrder.Remove(key);
            _usageOrder.Add(key);
        }
        else
        {
            // Add new key
            if (_cache.Count >= _capacity)
            {
                // Remove least recently used (first in list)
                TKey lruKey = _usageOrder[0];
                _usageOrder.RemoveAt(0);
                _cache.Remove(lruKey);
            }

            _cache[key] = value;
            _usageOrder.Add(key);
        }
        
        // Create snapshot after state change
        CreateSnapshot();
    }

    // Task 2.3: Rollback to n operations ago (0 = current state)
    public bool Rollback(int n)
    {
        if (n < 0 || n >= _history.Count)
        {
            return false; // Invalid rollback position
        }
        
        // Calculate target snapshot (from the end)
        int targetIndex = _history.Count - 1 - n;
        var targetSnapshot = _history[targetIndex];
        
        // Restore state
        _cache.Clear();
        _usageOrder.Clear();
        
        foreach (var kvp in targetSnapshot.CacheData)
        {
            _cache[kvp.Key] = kvp.Value;
        }
        
        foreach (var key in targetSnapshot.UsageOrder)
        {
            _usageOrder.Add(key);
        }
        
        // Remove newer snapshots
        int itemsToRemove = _history.Count - targetIndex - 1;
        if (itemsToRemove > 0)
        {
            _history.RemoveRange(targetIndex + 1, itemsToRemove);
        }
        
        return true;
    }

    // Helper method to see current state with history info
    public void PrintState()
    {
        Console.WriteLine($"Cache State (Count: {Count}/{Capacity}, History: {HistoryCount}):");
        Console.WriteLine("Usage Order (LRU -> MRU): " + string.Join(" -> ", _usageOrder));
        foreach (var kvp in _cache)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
        Console.WriteLine();
    }
    
    // Print history information
    public void PrintHistory()
    {
        Console.WriteLine($"History ({HistoryCount} snapshots):");
        for (int i = _history.Count - 1; i >= 0; i--)
        {
            var snapshot = _history[i];
            int stepsBack = _history.Count - 1 - i;
            Console.WriteLine($"  [{stepsBack}] {snapshot.Timestamp:HH:mm:ss} - {snapshot.CacheData.Count} items");
        }
        Console.WriteLine();
    }

    // Persistence and Recovery functionality
    public void SaveToFile(string filePath)
    {
        try
        {
            var persistData = new CachePersistData<TKey, TValue>
            {
                Capacity = _capacity,
                CacheData = _cache,
                UsageOrder = _usageOrder,
                History = _history.Select(h => new SerializableSnapshot<TKey, TValue>
                {
                    CacheData = h.CacheData,
                    UsageOrder = h.UsageOrder,
                    Timestamp = h.Timestamp
                }).ToList()
            };

            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                IncludeFields = true
            };
            
            // 確保目錄存在
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Console.WriteLine($"Created directory: {directory}");
            }
            
            string jsonString = JsonSerializer.Serialize(persistData, options);
            File.WriteAllText(filePath, jsonString);
            
            Console.WriteLine($"Cache saved to {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving cache: {ex.Message}");
            throw;
        }
    }

    public void LoadFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File {filePath} does not exist");
                return;
            }

            string jsonString = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions 
            { 
                IncludeFields = true
            };
            
            var persistData = JsonSerializer.Deserialize<CachePersistData<TKey, TValue>>(jsonString, options);
            
            if (persistData == null)
            {
                Console.WriteLine("Failed to deserialize cache data");
                return;
            }

            // Restore cache state
            _cache.Clear();
            _usageOrder.Clear();
            _history.Clear();

            foreach (var kvp in persistData.CacheData)
            {
                _cache[kvp.Key] = kvp.Value;
            }

            _usageOrder.AddRange(persistData.UsageOrder);

            foreach (var snapshot in persistData.History)
            {
                _history.Add(new CacheSnapshot<TKey, TValue>(snapshot.CacheData, snapshot.UsageOrder)
                {
                    Timestamp = snapshot.Timestamp
                });
            }

            Console.WriteLine($"Cache loaded from {filePath}");
            Console.WriteLine($"Restored: {Count} items, {HistoryCount} history snapshots");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading cache: {ex.Message}");
            throw;
        }
    }

    // 便利方法：保存到指定資料夾
    public void SaveToFolder(string folderName = "persistenceData", string fileName = "cache_state.json")
    {
        string filePath = Path.Combine(folderName, fileName);
        SaveToFile(filePath);
    }

    // 便利方法：從指定資料夾加載
    public void LoadFromFolder(string folderName = "persistenceData", string fileName = "cache_state.json")
    {
        string filePath = Path.Combine(folderName, fileName);
        LoadFromFile(filePath);
    }

    // 持久化：始終使用同一個文件（覆蓋模式）
    public void SavePersistent()
    {
        SaveToFolder(); // 使用預設參數：persistenceData/cache_state.json
    }

    // 從持久化文件加載
    public void LoadPersistent()
    {
        LoadFromFolder(); // 使用預設參數：persistenceData/cache_state.json
    }
}
