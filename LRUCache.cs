using System;
using System.Collections.Generic;
using System.Linq;

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
}
