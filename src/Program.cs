using LRUCache;

class Program
{
    static void Main(string[] args)
    {
        var capacity = 3;
        Console.WriteLine($"Initialize LRU Cache with capacity {capacity}");
        var cache = new LRUCache<string, int>(capacity, enablePersistence: true);
        
        cache.Put("A", 1);
        cache.Put("B", 2);
        cache.Put("C", 3);
        
        Console.WriteLine($"Get A: {cache.Get("A")}");
        Console.WriteLine($"Cache Size: {cache.Count}/{cache.Capacity}");
        
        cache.PrintState();
    }
}
