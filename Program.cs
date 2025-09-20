using System;
using LRUCache.Core;

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
        
        // 測試持久化功能
        Console.WriteLine("=== 持久化測試 ===");
        
        Console.WriteLine("保存當前緩存狀態...");
        cache.SavePersistent(); // 保存到 persistenceData/cache_state.json
        
        Console.WriteLine("\n清空緩存並添加新數據...");
        var newCache = new LRUCache<string, int>(3);
        newCache.Put("X", 100);
        newCache.Put("Y", 200);
        newCache.PrintState();
        
        Console.WriteLine("從持久化文件加載緩存狀態...");
        newCache.LoadPersistent(); // 從 persistenceData/cache_state.json 加載
        newCache.PrintState();
        newCache.PrintHistory();
        
        Console.WriteLine("驗證加載的數據是否正確...");
        Console.WriteLine($"測試 Get A: {newCache.Get("A")}");
        Console.WriteLine($"測試 Get C: {newCache.Get("C")}");
        
        Console.WriteLine($"\n持久化文件: persistenceData/cache_state.json");
        Console.WriteLine("每次保存都會覆蓋同一個文件，保持簡潔！");
        
        // 可選：清理測試文件  
        // try
        // {
        //     if (System.IO.File.Exists(saveFile))
        //     {
        //         System.IO.File.Delete(saveFile);
        //         Console.WriteLine($"\n清理測試文件: {saveFile}");
        //     }
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine($"清理文件時出錯: {ex.Message}");
        // }
    }
}
