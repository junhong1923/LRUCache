> 📌 **版本說明**  
> - `v1.0` (Tag)：初版，作業 deadline 交付的版本  
> - `main`：優化後的最新版本

## LRU Cache Implementation Features

- LRU behavior with Get/Put operations and eviction policy
- History tracking and rollback to any previous state
- Persistence functionality for state recovery after shutdown
- Generic support for any key-value types
- Configurable persistence with constructor parameter
- Automatic data loading on initialization

## Project Structure

```
LRUCache/
├── src/                             # Source code
│   ├── Models/
│   │   ├── CacheSnapshot.cs         # Snapshot class for rollback functionality
│   │   └── PersistenceModels.cs     # Data structures for persistence
│   ├── CacheInterface.cs            # Cache interface definition
│   ├── LRUCache.cs                  # Core LRU cache implementation
│   ├── LRUCache.csproj              # Project file
│   ├── LRUCache.sln                 # Solution file
│   ├── Program.cs                   # Demonstration program
│   └── persistenceData/             # Persistence storage directory
│       └── cache_state.json         # Cache state file (created when needed)
├── test/                            # Test projects
│   └── LRUCache.Tests/
│       ├── LRUCache.Tests.csproj    # Test project file
│       └── LRUCacheTest.cs          # Unit tests
└── README.md
```

## Persistence Mechanism

The LRU Cache supports optional persistence functionality:

- **Storage Location**: `src/persistenceData/cache_state.json`
- **Format**: JSON serialization using System.Text.Json
- **Automatic Loading**: When persistence is enabled, cache automatically loads existing data during initialization
- **Auto-Save**: Cache state is automatically saved after each Put operation
- **Configuration**: Controlled via constructor parameter `enablePersistence` (default: true)

### Sample Persistence File

Here's an example of what the `cache_state.json` file looks like:

```json
{
  "Capacity": 3,
  "CacheData": {
    "A": 1,
    "B": 2
  },
  "UsageOrder": [
    "A",
    "B"
  ],
  "History": [
    {
      "CacheData": {},
      "UsageOrder": [],
      "Timestamp": "2025-09-20T12:02:46.490473+08:00"
    },
    {
      "CacheData": {
        "A": 1
      },
      "UsageOrder": [
        "A"
      ],
      "Timestamp": "2025-09-20T12:02:46.4975656+08:00"
    },
    {
      "CacheData": {
        "A": 1,
        "B": 2
      },
      "UsageOrder": [
        "A",
        "B"
      ],
      "Timestamp": "2025-09-20T12:02:46.4975751+08:00"
    }
  ]
}
```

**File Structure Explanation:**
- `Capacity`: Maximum number of items the cache can hold
- `CacheData`: Current key-value pairs in the cache
- `UsageOrder`: Keys ordered by usage (most recently used at the end)
- `History`: Array of snapshots for rollback functionality, each containing:
  - `CacheData`: Cache state at that point in time
  - `UsageOrder`: Usage order at that point in time
  - `Timestamp`: When the snapshot was created

### Persistence Features
- Preserves cache data across application restarts
- Maintains LRU order and capacity settings

## Getting Started

### Requirements

- .NET 9.0

### Build and Run

1. Build the main project
   ```bash
   cd src
   dotnet build
   ```

2. Run the demonstration
   ```bash
   cd src
   dotnet run
   ```

### Testing

1. Run all unit tests
   ```bash
   cd test/LRUCache.Tests
   dotnet test
   ```

2. Run tests with detailed output
   ```bash
   cd test/LRUCache.Tests
   dotnet test --verbosity normal
   ```

3. Run specific test
   ```bash
   cd test/LRUCache.Tests
   dotnet test --filter "TestMethodName"
   ```

### TODO

1. thread-safe situation