using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("LRU Cache with History & Rollback Demo\n");
        
        var cache = new LRUCache<string, int>(3);
        
        // 基本操作
        Console.WriteLine("=== 基本操作 ===");
        cache.Put("A", 1);
        cache.Put("B", 2);
        cache.Put("C", 3);
        cache.PrintState();
        
        // 測試 Get 操作
        Console.WriteLine("=== Get 操作 ===");
        Console.WriteLine($"A = {cache.Get("A")}");
        cache.PrintState();
        
        // 測試 LRU 淘汰
        Console.WriteLine("=== LRU 淘汰 ===");
        cache.Put("D", 4);
        cache.PrintState();
        
        // 顯示歷史記錄
        Console.WriteLine("=== 歷史記錄 ===");
        cache.PrintHistory();
        
        // 測試回滾功能
        Console.WriteLine("=== 回滾測試 ===");
        Console.WriteLine("回滾到 2 步之前:");
        cache.Rollback(2);
        cache.PrintState();
        
        Console.WriteLine("回滾到 1 步之前:");
        cache.Rollback(1);
        cache.PrintState();
        
        cache.PrintHistory();
    }
}
