# File Operations

> **Category:** Advanced | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

Safe file operations for widgets including reading, writing, watching, and managing widget data files.

---

## Widget Data Paths

```csharp
public static class WidgetPaths
{
    private static readonly string WidgetKey = "my-widget";
    
    /// <summary>
    /// Base data directory: %APPDATA%\3SC\Widgets\Community\{widget-key}
    /// </summary>
    public static string DataDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "3SC", "Widgets", "Community", WidgetKey);
    
    /// <summary>
    /// Settings file path
    /// </summary>
    public static string SettingsFile => Path.Combine(DataDirectory, "settings.json");
    
    /// <summary>
    /// Log directory
    /// </summary>
    public static string LogDirectory => Path.Combine(DataDirectory, "logs");
    
    /// <summary>
    /// Cache directory
    /// </summary>
    public static string CacheDirectory => Path.Combine(DataDirectory, "cache");
    
    /// <summary>
    /// Ensure all directories exist
    /// </summary>
    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(LogDirectory);
        Directory.CreateDirectory(CacheDirectory);
    }
}
```

---

## Safe File Reading

```csharp
public static class FileHelper
{
    public static async Task<string?> ReadTextSafeAsync(string path, ILogger log)
    {
        try
        {
            if (!File.Exists(path))
            {
                log.Debug("File not found: {Path}", path);
                return null;
            }
            
            return await File.ReadAllTextAsync(path);
        }
        catch (IOException ex)
        {
            log.Warning(ex, "Failed to read file: {Path}", path);
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            log.Warning(ex, "Access denied reading file: {Path}", path);
            return null;
        }
    }
    
    public static async Task<T?> ReadJsonSafeAsync<T>(string path, ILogger log) where T : class
    {
        var json = await ReadTextSafeAsync(path, log);
        if (string.IsNullOrEmpty(json)) return null;
        
        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException ex)
        {
            log.Warning(ex, "Failed to parse JSON: {Path}", path);
            return null;
        }
    }
}
```

---

## Atomic File Writing

```csharp
public static class FileHelper
{
    /// <summary>
    /// Write file atomically using temp file + rename pattern.
    /// Prevents corruption if process crashes during write.
    /// </summary>
    public static async Task WriteTextAtomicAsync(string path, string content, ILogger log)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        var tempPath = path + ".tmp";
        var backupPath = path + ".bak";
        
        try
        {
            // Write to temp file
            await File.WriteAllTextAsync(tempPath, content);
            
            // Backup existing file
            if (File.Exists(path))
            {
                File.Copy(path, backupPath, overwrite: true);
            }
            
            // Atomic rename
            File.Move(tempPath, path, overwrite: true);
            
            // Remove backup on success
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
            
            log.Debug("File written: {Path}", path);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Failed to write file: {Path}", path);
            
            // Restore from backup if available
            if (File.Exists(backupPath) && !File.Exists(path))
            {
                File.Move(backupPath, path);
            }
            
            throw;
        }
        finally
        {
            // Cleanup temp file
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }
        }
    }
    
    public static async Task WriteJsonAtomicAsync<T>(string path, T data, ILogger log)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(data, options);
        await WriteTextAtomicAsync(path, json, log);
    }
}
```

---

## File Watcher Service

```csharp
public class FileWatcherService : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly ILogger _log;
    private readonly Action<string> _onChanged;
    private readonly Timer _debounceTimer;
    private string? _pendingPath;
    
    public FileWatcherService(string directory, string filter, Action<string> onChanged, ILogger log)
    {
        _log = log;
        _onChanged = onChanged;
        
        _debounceTimer = new Timer(OnDebounceElapsed, null, Timeout.Infinite, Timeout.Infinite);
        
        _watcher = new FileSystemWatcher(directory, filter)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = true
        };
        
        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Renamed += OnFileRenamed;
        _watcher.Error += OnError;
        
        _log.Information("Watching {Directory} for {Filter}", directory, filter);
    }
    
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce rapid changes
        _pendingPath = e.FullPath;
        _debounceTimer.Change(100, Timeout.Infinite);
    }
    
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        _pendingPath = e.FullPath;
        _debounceTimer.Change(100, Timeout.Infinite);
    }
    
    private void OnDebounceElapsed(object? state)
    {
        if (_pendingPath != null)
        {
            _log.Debug("File changed: {Path}", _pendingPath);
            
            try
            {
                _onChanged(_pendingPath);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error handling file change");
            }
        }
    }
    
    private void OnError(object sender, ErrorEventArgs e)
    {
        _log.Error(e.GetException(), "File watcher error");
    }
    
    public void Dispose()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _debounceTimer.Dispose();
    }
}
```

---

## Directory Operations

```csharp
public static class DirectoryHelper
{
    /// <summary>
    /// Get total size of directory in bytes
    /// </summary>
    public static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path)) return 0;
        
        return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
            .Sum(f => new FileInfo(f).Length);
    }
    
    /// <summary>
    /// Clean old files from directory
    /// </summary>
    public static void CleanOldFiles(string path, TimeSpan maxAge, ILogger log)
    {
        if (!Directory.Exists(path)) return;
        
        var cutoff = DateTime.Now - maxAge;
        var files = Directory.GetFiles(path)
            .Select(f => new FileInfo(f))
            .Where(f => f.LastWriteTime < cutoff)
            .ToList();
        
        foreach (var file in files)
        {
            try
            {
                file.Delete();
                log.Debug("Deleted old file: {Name}", file.Name);
            }
            catch (Exception ex)
            {
                log.Warning(ex, "Failed to delete: {Name}", file.Name);
            }
        }
    }
    
    /// <summary>
    /// Copy directory recursively
    /// </summary>
    public static void CopyDirectory(string source, string destination, bool overwrite = false)
    {
        Directory.CreateDirectory(destination);
        
        foreach (var file in Directory.GetFiles(source))
        {
            var destFile = Path.Combine(destination, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite);
        }
        
        foreach (var dir in Directory.GetDirectories(source))
        {
            var destDir = Path.Combine(destination, Path.GetFileName(dir));
            CopyDirectory(dir, destDir, overwrite);
        }
    }
}
```

---

## File Dialog Integration

```csharp
public static class FileDialogs
{
    public static string? OpenFile(string title, string filter)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = title,
            Filter = filter,
            CheckFileExists = true
        };
        
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
    
    public static string[]? OpenFiles(string title, string filter)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = title,
            Filter = filter,
            Multiselect = true,
            CheckFileExists = true
        };
        
        return dialog.ShowDialog() == true ? dialog.FileNames : null;
    }
    
    public static string? SaveFile(string title, string filter, string? defaultName = null)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = title,
            Filter = filter,
            FileName = defaultName ?? ""
        };
        
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
    
    public static string? SelectFolder(string title)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = title,
            UseDescriptionForTitle = true
        };
        
        return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK 
            ? dialog.SelectedPath 
            : null;
    }
}

// Common filters
public static class FileFilters
{
    public const string Images = "Images|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp|All Files|*.*";
    public const string Videos = "Videos|*.mp4;*.mkv;*.avi;*.mov;*.wmv|All Files|*.*";
    public const string Documents = "Documents|*.pdf;*.doc;*.docx;*.txt|All Files|*.*";
    public const string Json = "JSON Files|*.json|All Files|*.*";
    public const string All = "All Files|*.*";
}
```

---

## Best Practices

1. **Use atomic writes** - Temp file + rename prevents corruption
2. **Handle exceptions** - IO operations can fail unexpectedly
3. **Debounce file watchers** - Rapid changes cause multiple events
4. **Validate paths** - Prevent directory traversal attacks
5. **Clean up temp files** - Don't leave orphaned files

---

## Related Skills

- [data-persistence.md](../data/data-persistence.md) - Data storage patterns
- [input-validation.md](../security/input-validation.md) - Path validation

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added file watcher |
| 1.0.0 | 2025-06-01 | Initial version |
