using System.Text.Json;
using LRUCache.Models;

namespace LRUCache
{
    /// <summary>
    /// LRU (Least Recently Used) Cache
    /// </summary>
    public class LRUCache<TKey, TValue> : CacheInterface<TKey, TValue>
        where TKey : notnull
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, TValue> _cache;
        private readonly List<TKey> _usageOrder; // Most recently used at the end
        private readonly List<CacheSnapshot<TKey, TValue>> _history; // History for rollback

        private readonly string _folderName;
        private readonly string _fileName;
        private readonly bool _enablePersistence;

        /// <summary>
        /// Constructor
        /// </summary>
        public LRUCache(int capacity, bool enablePersistence = true)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must greater than 0");
            }
        
            _capacity = capacity;
            _cache = new Dictionary<TKey, TValue>();
            _usageOrder = new List<TKey>();
            _history = new List<CacheSnapshot<TKey, TValue>>();
            _enablePersistence = enablePersistence;

            // File path: .\LRUCache\src\persistenceData
            _folderName = "../../../persistenceData";
            _fileName = "cache_state.json";

            // Load existing data if persistence is enabled
            if (_enablePersistence)
            {
                TryLoadExistingData();
            }

            // Create initial snapshot if no data loaded
            if (_history.Count == 0)
            {
                CreateSnapshot();
            }
        }

        public int Count => _cache.Count;
        public int Capacity => _capacity;
        public int HistoryCount => _history.Count;
        public List<TKey> UsageOrder => _usageOrder;
  
        /// <summary>
        /// Get method - retrieve value and update usage order
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <returns>Cache Value of Key</returns>
        public TValue? Get(TKey key)
        {
            if (!_cache.TryGetValue(key, out TValue? value))
            {
                return default;
            }

            // Update the usage order
            _usageOrder.Remove(key);
            _usageOrder.Add(key); // Move to end (most recently used)
        
            // Create snapshot after state change
            CreateSnapshot();

            return value;
        }

        /// <summary>
        /// Put method - add/update value and manage capacity
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cache value</param>
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
      
        /// <summary>
        /// Rollback to n operations ago (0 = current state) 
        /// </summary>
        /// <param name="n">The n.</param>
        /// <returns>rollback result</returns>
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

        /// <summary>
        /// Restore the cache state from persisted data
        /// </summary>
        public void Load()
        {
            var filePath = Path.Combine(_folderName, _fileName);
            this.LoadFromFile(filePath);
        }

        /// <summary>
        /// Save the cache data persistently into file
        /// </summary>
        /// <remarks>Overwrite the existing file</remarks>
        public void SavePersistent()
        {
            var filePath = Path.Combine(_folderName, _fileName);
            this.SaveToFile(filePath);
        }

        /// <summary>
        /// Prints current state with history info
        /// </summary>
        /// <remarks>For console testing purposes</remarks>
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

        /// <summary>
        /// Create a snapshot of current state
        /// </summary>
        private void CreateSnapshot()
        {
            var snapshot = new CacheSnapshot<TKey, TValue>(_cache, _usageOrder);
            _history.Add(snapshot);

            // save only when persistence is enabled
            if (_enablePersistence)
            {
                this.SavePersistent();
            }
        }

        /// <summary>
        /// Try to load existing persistent data if available
        /// </summary>
        private void TryLoadExistingData()
        {
            try
            {
                var filePath = Path.Combine(_folderName, _fileName);
                if (File.Exists(filePath))
                {
                    LoadFromFile(filePath);
                    Console.WriteLine("Existing persistent data was loaded...");
                }
                else
                {
                    Console.WriteLine("Existing persistent data not found...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Catch exception: {ex.Message}");

                _cache.Clear();
                _usageOrder.Clear();
                _history.Clear();
            }
        }

        /// <summary>
        /// Saves cache data into file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        private void SaveToFile(string filePath)
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

                // Check and create directory if not exists
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

        private void LoadFromFile(string filePath)
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
    }
}
