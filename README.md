## LRU Cache Implementation Features

- LRU behavior with Get/Put operations and eviction policy
- History tracking and rollback to any previous state
- Persistence functionality for state recovery after shutdown
- Generic support for any key-value types

## Project Structure

```
LRUCache/
├── Models/
│   ├── CacheSnapshot.cs             # Snapshot class for rollback functionality
│   └── PersistenceModels.cs         # Data structures for persistence
├── LRUCache.cs                      # Core LRU cache implementation
├── Program.cs                       # Test and demonstration program
├── persistenceData/
│   └── cache_state.json            # Cache state file
└── README.md
```

## Getting Started

### Requirements

- .NET 9.0

### Usage

1. Build the project
   ```bash
   dotnet build
   ```

2. Run the demonstration
   ```bash
   dotnet run
   ```