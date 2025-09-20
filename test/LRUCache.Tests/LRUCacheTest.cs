namespace LRUCache.Tests
{
    public class LRUCacheTest
    {
        [Fact]
        public void BasicOperations_ShouldWorkCorrectly()
        {
            // Arrange
            var cache = this.GetTargetInstance<string, int>(3);

            // Act
            cache.Put("A", 1);
            cache.Put("B", 2);
            cache.Put("C", 3);

            // Assert
            Assert.Equal(3, cache.Count);
            Assert.Equal(1, cache.Get("A"));
            Assert.Equal(2, cache.Get("B"));
            Assert.Equal(3, cache.Get("C"));
        }

        [Fact]
        public void LRUEviction_ShouldRemoveLeastRecentlyUsedItem()
        {
            // Arrange
            var cache = this.GetTargetInstance<string, int>(3);
            cache.Put("A", 1);
            cache.Put("B", 2);
            cache.Put("C", 3);
            
            // Act - Access A, making it most recently used
            var valueA = cache.Get("A");
            cache.Put("D", 4); // This should evict B (least recently used)
            
            // Assert
            Assert.Equal(1, valueA);
            Assert.Equal(1, cache.Get("A")); // A should still be there
            Assert.Equal(3, cache.Get("C")); // C should still be there
            Assert.Equal(4, cache.Get("D")); // D should be there
            Assert.Equal(default(int), cache.Get("B")); // B should be evicted
        }

        [Fact]
        public void UpdateExistingKey_ShouldUpdateValueAndUsageOrder()
        {
            // Arrange
            var cache = this.GetTargetInstance<string, int>(3);
            cache.Put("A", 1);
            cache.Put("B", 2);
            cache.Put("C", 3);
            
            // Act - Update existing key
            cache.Put("A", 10); // Update A's value
            cache.Put("D", 4);  // This should evict B (now least recently used)
            
            // Assert
            Assert.Equal(10, cache.Get("A")); // A should have updated value
            Assert.Equal(default(int), cache.Get("B")); // B should be evicted
            Assert.Equal(3, cache.Get("C")); // C should still be there
            Assert.Equal(4, cache.Get("D")); // D should be there
        }

        [Fact]
        public void HistoryTracking_ShouldRecordOperations()
        {
            // Arrange
            var cache = this.GetTargetInstance<string, int>(2);

            // Act
            cache.Put("A", 1); // History: 1 snapshot (initial) + 1 (after put)
            cache.Put("B", 2); // History: +1 snapshot
            cache.Get("A");    // History: +1 snapshot
            
            // Assert
            Assert.True(cache.HistoryCount > 0);
            Assert.True(cache.HistoryCount >= 4); // At least initial + 3 operations
        }

        [Fact]
        public void Rollback_ShouldRestorePreviousState()
        {
            // Arrange
            var cache = this.GetTargetInstance<string, int>(3);
            cache.Put("A", 1);
            cache.Put("B", 2);
            cache.Put("C", 3);

            // Act
            cache.Put("D", 4); // A would be evicted
            var beforeRollback = cache.Get("A"); // Should be default

            cache.Rollback(2);
            var orderAfterRollback = new List<string> { "A", "B"}; // Rollback 2 steps, Order should be [A,B]

            // Assert
            Assert.Equal(default(int), beforeRollback);
            Assert.Equal(orderAfterRollback, cache.UsageOrder);
        }

        [Fact]
        public void Rollback_MultipleTimes_ShouldWorkCorrectly()
        {
            // Arrange
            var cache = this.GetTargetInstance<string, int>(2);
            cache.Put("A", 1);
            cache.Put("B", 2);
            cache.Put("C", 3);

            var orderBeforeRollback = new List<string> { "B", "C" };
            var orderAfterRollback = new List<string> { "A", "B" };
            var orderAfterRollbackTwice = new List<string> { "A" };

            // Act & Assert
            Assert.Equal(default(int), cache.Get("A")); // After C is added, A should be evicted
            Assert.Equal(orderBeforeRollback, cache.UsageOrder); // B,C should still in cache, and usage order should be [B,C]

            cache.Rollback(1);
            Assert.Equal(default(int), cache.Get("C")); // C should be gone after rollback
            Assert.Equal(orderAfterRollback, cache.UsageOrder); // A,B should be in cache, and usage order should be [A,B]

            //// After another rollback, we should be further back
            cache.Rollback(1);
            Assert.Equal(1, cache.Count);
            Assert.Equal(default(int), cache.Get("B")); // B should be gone after another rollback
            Assert.Equal(orderAfterRollbackTwice, cache.UsageOrder); // Only A should remain
        }

        [Fact]
        public void Constructor_WithZeroOrNegativeCapacity_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => this.GetTargetInstance<string, int>(0));
            Assert.Throws<ArgumentException>(() => this.GetTargetInstance<string, decimal>(-1));
        }

        [Fact]
        public void Get_NonExistentKey_ShouldReturnDefault()
        {
            // Arrange
            var cache = this.GetTargetInstance<string, int>(3);

            // Act & Assert
            Assert.Equal(default(int), cache.Get("NonExistent"));
        }

        [Fact]
        public void Capacity_ShouldReturnCorrectValue()
        {
            // Arrange & Act
            var cache = this.GetTargetInstance<string, int>(5);
            
            // Assert
            Assert.Equal(5, cache.Capacity);
        }

        private LRUCache<TKey, TValue> GetTargetInstance<TKey, TValue>(int capacity)
        {
            return new LRUCache<TKey, TValue>(capacity, enablePersistence: false);
        }
    }
}
