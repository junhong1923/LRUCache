using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("LRU Cache Demo");
        
        var cache = new LRUCache<string, int>(3);
        
        cache.Put("A", 1);
        cache.Put("B", 2);
        cache.Put("C", 3);
        
        Console.WriteLine($"A = {cache.Get("A")}");
        Console.WriteLine($"Count = {cache.Count}");
        
        cache.Put("D", 4);
        Console.WriteLine($"B = {cache.Get("B")}");
        
        cache.PrintState();
    }
}
