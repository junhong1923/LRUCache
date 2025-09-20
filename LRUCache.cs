using System;
using System.Collections.Generic;

// Simple LRU Cache implementation using Dictionary + List
public class LRUCache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, TValue> _cache;
    private readonly List<TKey> _usageOrder; // Most recently used at the end

    public LRUCache(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(capacity));
        
        _capacity = capacity;
        _cache = new Dictionary<TKey, TValue>();
        _usageOrder = new List<TKey>();
    }

    public int Count => _cache.Count;
    public int Capacity => _capacity;

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
    }

    // Helper method to see current state
    public void PrintState()
    {
        Console.WriteLine($"Cache State (Count: {Count}/{Capacity}):");
        Console.WriteLine("Usage Order (LRU -> MRU): " + string.Join(" -> ", _usageOrder));
        foreach (var kvp in _cache)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
        Console.WriteLine();
    }
}
