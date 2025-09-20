namespace LRUCache
{
    /// <summary>
    /// Interface for Cache
    /// </summary>
    public interface CacheInterface<TKey, TValue>
    {
        TValue? Get(TKey key);

        void Put(TKey key, TValue value);

        bool Rollback(int steps);

        void Load();
    }
}
