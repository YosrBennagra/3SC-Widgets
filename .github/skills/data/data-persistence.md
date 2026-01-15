# Data Persistence

> **Category:** Data | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers persisting widget data beyond simple settings - including state, user content, and cached data.

## Prerequisites

- [settings-management.md](settings-management.md)
- [serialization.md](serialization.md)

---

## Storage Locations

```
%APPDATA%\3SC\Widgets\{widget-key}\
├── settings.json       # User preferences
├── state.json          # Runtime state
├── data\               # Widget-specific data
│   ├── notes.json      # Content data
│   ├── cache\          # Cached API responses
│   └── exports\        # User exports
└── logs\               # Log files
```

### Path Utilities

```csharp
public static class DataPaths
{
    private static readonly string BasePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "3SC", "Widgets");
    
    public static string GetWidgetPath(string widgetKey) =>
        Path.Combine(BasePath, widgetKey);
    
    public static string GetDataPath(string widgetKey) =>
        Path.Combine(GetWidgetPath(widgetKey), "data");
    
    public static string GetCachePath(string widgetKey) =>
        Path.Combine(GetDataPath(widgetKey), "cache");
    
    public static void EnsureDirectories(string widgetKey)
    {
        Directory.CreateDirectory(GetWidgetPath(widgetKey));
        Directory.CreateDirectory(GetDataPath(widgetKey));
        Directory.CreateDirectory(GetCachePath(widgetKey));
    }
}
```

---

## State Persistence

For runtime state that should survive restarts:

```csharp
/// <summary>
/// Widget state that persists between sessions.
/// </summary>
public class WidgetState
{
    [JsonPropertyName("lastRefresh")]
    public DateTime? LastRefresh { get; set; }
    
    [JsonPropertyName("expandedSections")]
    public List<string> ExpandedSections { get; set; } = new();
    
    [JsonPropertyName("selectedTab")]
    public int SelectedTab { get; set; }
    
    [JsonPropertyName("scrollPosition")]
    public double ScrollPosition { get; set; }
}

/// <summary>
/// Service for managing widget state.
/// </summary>
public class StateService
{
    private readonly string _statePath;
    private WidgetState _state = new();
    
    public StateService(string widgetKey)
    {
        _statePath = Path.Combine(DataPaths.GetWidgetPath(widgetKey), "state.json");
        Load();
    }
    
    public WidgetState State => _state;
    
    public void Load()
    {
        try
        {
            if (File.Exists(_statePath))
            {
                var json = File.ReadAllText(_statePath);
                _state = JsonSerializer.Deserialize<WidgetState>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load state");
            _state = new WidgetState();
        }
    }
    
    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(_statePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            
            var json = JsonSerializer.Serialize(_state, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(_statePath, json);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save state");
        }
    }
}
```

---

## File-Based Data Storage

### Notes Widget Example

```csharp
public class NotesDataService
{
    private readonly string _notesPath;
    
    public NotesDataService(string widgetKey)
    {
        _notesPath = Path.Combine(DataPaths.GetDataPath(widgetKey), "notes.json");
    }
    
    public async Task<List<Note>> LoadNotesAsync()
    {
        try
        {
            if (!File.Exists(_notesPath))
                return new List<Note>();
            
            var json = await File.ReadAllTextAsync(_notesPath);
            return JsonSerializer.Deserialize<List<Note>>(json) ?? new();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load notes");
            return new List<Note>();
        }
    }
    
    public async Task SaveNotesAsync(List<Note> notes)
    {
        try
        {
            var directory = Path.GetDirectoryName(_notesPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            
            var json = JsonSerializer.Serialize(notes, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            // Write to temp file first, then rename (atomic operation)
            var tempPath = _notesPath + ".tmp";
            await File.WriteAllTextAsync(tempPath, json);
            File.Move(tempPath, _notesPath, overwrite: true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save notes");
        }
    }
    
    public async Task<Note?> GetNoteAsync(Guid id)
    {
        var notes = await LoadNotesAsync();
        return notes.FirstOrDefault(n => n.Id == id);
    }
    
    public async Task AddNoteAsync(Note note)
    {
        var notes = await LoadNotesAsync();
        notes.Add(note);
        await SaveNotesAsync(notes);
    }
    
    public async Task UpdateNoteAsync(Note note)
    {
        var notes = await LoadNotesAsync();
        var index = notes.FindIndex(n => n.Id == note.Id);
        if (index >= 0)
        {
            notes[index] = note;
            await SaveNotesAsync(notes);
        }
    }
    
    public async Task DeleteNoteAsync(Guid id)
    {
        var notes = await LoadNotesAsync();
        notes.RemoveAll(n => n.Id == id);
        await SaveNotesAsync(notes);
    }
}

public class Note
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("modifiedAt")]
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();
}
```

---

## Caching

### Simple File Cache

```csharp
public class CacheService
{
    private readonly string _cachePath;
    private readonly TimeSpan _defaultExpiry;
    
    public CacheService(string widgetKey, TimeSpan? defaultExpiry = null)
    {
        _cachePath = DataPaths.GetCachePath(widgetKey);
        _defaultExpiry = defaultExpiry ?? TimeSpan.FromHours(1);
        Directory.CreateDirectory(_cachePath);
    }
    
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var entry = await GetEntryAsync<T>(key);
        return entry?.Data;
    }
    
    public async Task<CacheEntry<T>?> GetEntryAsync<T>(string key) where T : class
    {
        var filePath = GetFilePath(key);
        
        if (!File.Exists(filePath))
            return null;
        
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var entry = JsonSerializer.Deserialize<CacheEntry<T>>(json);
            
            if (entry == null)
                return null;
            
            // Check expiry
            if (entry.ExpiresAt < DateTime.UtcNow)
            {
                File.Delete(filePath);
                return null;
            }
            
            return entry;
        }
        catch
        {
            return null;
        }
    }
    
    public async Task SetAsync<T>(string key, T data, TimeSpan? expiry = null)
    {
        var entry = new CacheEntry<T>
        {
            Data = data,
            CachedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow + (expiry ?? _defaultExpiry)
        };
        
        var filePath = GetFilePath(key);
        var json = JsonSerializer.Serialize(entry);
        await File.WriteAllTextAsync(filePath, json);
    }
    
    public void Remove(string key)
    {
        var filePath = GetFilePath(key);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
    
    public void Clear()
    {
        foreach (var file in Directory.GetFiles(_cachePath, "*.cache"))
        {
            File.Delete(file);
        }
    }
    
    public void ClearExpired()
    {
        foreach (var file in Directory.GetFiles(_cachePath, "*.cache"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var entry = JsonSerializer.Deserialize<CacheEntryBase>(json);
                
                if (entry?.ExpiresAt < DateTime.UtcNow)
                    File.Delete(file);
            }
            catch
            {
                // Delete corrupted cache files
                File.Delete(file);
            }
        }
    }
    
    private string GetFilePath(string key)
    {
        // Sanitize key for filename
        var safeKey = string.Join("_", key.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_cachePath, $"{safeKey}.cache");
    }
}

public class CacheEntry<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }
    
    [JsonPropertyName("cachedAt")]
    public DateTime CachedAt { get; set; }
    
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
}

public class CacheEntryBase
{
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
}
```

### Usage Example

```csharp
public class WeatherService
{
    private readonly CacheService _cache;
    private readonly HttpClient _httpClient;
    
    public WeatherService(string widgetKey)
    {
        _cache = new CacheService(widgetKey, TimeSpan.FromMinutes(15));
        _httpClient = new HttpClient();
    }
    
    public async Task<WeatherData?> GetWeatherAsync(string city)
    {
        var cacheKey = $"weather_{city}";
        
        // Try cache first
        var cached = await _cache.GetAsync<WeatherData>(cacheKey);
        if (cached != null)
            return cached;
        
        // Fetch from API
        var response = await _httpClient.GetAsync($"https://api.weather.example/current?city={city}");
        if (!response.IsSuccessStatusCode)
            return null;
        
        var json = await response.Content.ReadAsStringAsync();
        var weather = JsonSerializer.Deserialize<WeatherData>(json);
        
        // Cache result
        if (weather != null)
            await _cache.SetAsync(cacheKey, weather, TimeSpan.FromMinutes(15));
        
        return weather;
    }
}
```

---

## SQLite for Complex Data

For widgets with complex data needs:

```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.0" />
```

```csharp
using Microsoft.Data.Sqlite;

public class DatabaseService : IDisposable
{
    private readonly string _dbPath;
    private SqliteConnection? _connection;
    
    public DatabaseService(string widgetKey)
    {
        var dataPath = DataPaths.GetDataPath(widgetKey);
        Directory.CreateDirectory(dataPath);
        _dbPath = Path.Combine(dataPath, "data.db");
    }
    
    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection($"Data Source={_dbPath}");
        await _connection.OpenAsync();
        
        // Create tables
        var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS items (
                id TEXT PRIMARY KEY,
                title TEXT NOT NULL,
                content TEXT,
                created_at TEXT NOT NULL,
                modified_at TEXT NOT NULL
            );
            
            CREATE INDEX IF NOT EXISTS idx_items_modified 
            ON items(modified_at DESC);
        ";
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task<List<Item>> GetItemsAsync(int limit = 100)
    {
        var items = new List<Item>();
        
        var command = _connection!.CreateCommand();
        command.CommandText = @"
            SELECT id, title, content, created_at, modified_at 
            FROM items 
            ORDER BY modified_at DESC 
            LIMIT @limit
        ";
        command.Parameters.AddWithValue("@limit", limit);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new Item
            {
                Id = Guid.Parse(reader.GetString(0)),
                Title = reader.GetString(1),
                Content = reader.IsDBNull(2) ? null : reader.GetString(2),
                CreatedAt = DateTime.Parse(reader.GetString(3)),
                ModifiedAt = DateTime.Parse(reader.GetString(4))
            });
        }
        
        return items;
    }
    
    public async Task SaveItemAsync(Item item)
    {
        var command = _connection!.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO items (id, title, content, created_at, modified_at)
            VALUES (@id, @title, @content, @created_at, @modified_at)
        ";
        command.Parameters.AddWithValue("@id", item.Id.ToString());
        command.Parameters.AddWithValue("@title", item.Title);
        command.Parameters.AddWithValue("@content", item.Content ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@created_at", item.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@modified_at", item.ModifiedAt.ToString("O"));
        
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task DeleteItemAsync(Guid id)
    {
        var command = _connection!.CreateCommand();
        command.CommandText = "DELETE FROM items WHERE id = @id";
        command.Parameters.AddWithValue("@id", id.ToString());
        await command.ExecuteNonQueryAsync();
    }
    
    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}
```

---

## Atomic File Operations

Always use atomic writes for important data:

```csharp
public static class AtomicFile
{
    public static async Task WriteAllTextAsync(string path, string content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        
        var tempPath = path + $".{Guid.NewGuid():N}.tmp";
        
        try
        {
            await File.WriteAllTextAsync(tempPath, content);
            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            // Clean up temp file if move failed
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
    
    public static async Task WriteAllBytesAsync(string path, byte[] data)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        
        var tempPath = path + $".{Guid.NewGuid():N}.tmp";
        
        try
        {
            await File.WriteAllBytesAsync(tempPath, data);
            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}
```

---

## Backup & Recovery

```csharp
public class BackupService
{
    private readonly string _dataPath;
    private readonly string _backupPath;
    
    public BackupService(string widgetKey)
    {
        _dataPath = DataPaths.GetDataPath(widgetKey);
        _backupPath = Path.Combine(_dataPath, "backups");
        Directory.CreateDirectory(_backupPath);
    }
    
    public async Task CreateBackupAsync()
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var backupFile = Path.Combine(_backupPath, $"backup_{timestamp}.zip");
        
        using var archive = ZipFile.Open(backupFile, ZipArchiveMode.Create);
        
        foreach (var file in Directory.GetFiles(_dataPath, "*.json"))
        {
            archive.CreateEntryFromFile(file, Path.GetFileName(file));
        }
        
        Log.Information("Backup created: {Path}", backupFile);
        
        // Keep only last 5 backups
        CleanupOldBackups(5);
    }
    
    public async Task RestoreBackupAsync(string backupFile)
    {
        // Create safety backup first
        await CreateBackupAsync();
        
        // Extract backup
        ZipFile.ExtractToDirectory(backupFile, _dataPath, overwriteFiles: true);
        
        Log.Information("Backup restored: {Path}", backupFile);
    }
    
    public List<string> GetAvailableBackups()
    {
        return Directory.GetFiles(_backupPath, "backup_*.zip")
            .OrderByDescending(f => f)
            .ToList();
    }
    
    private void CleanupOldBackups(int keepCount)
    {
        var backups = GetAvailableBackups();
        foreach (var backup in backups.Skip(keepCount))
        {
            File.Delete(backup);
        }
    }
}
```

---

## Best Practices

1. **Use atomic writes** for important data
2. **Cache expensive operations** with appropriate TTL
3. **Clean up old data** periodically
4. **Handle corrupted files** gracefully
5. **Backup before destructive operations**

---

## Related Skills

- [settings-management.md](settings-management.md) - Settings patterns
- [serialization.md](serialization.md) - JSON serialization
- [error-handling.md](../quality/error-handling.md) - Error handling

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added SQLite, caching, backups |
| 1.0.0 | 2025-06-01 | Initial version |
