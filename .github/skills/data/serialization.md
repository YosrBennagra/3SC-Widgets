# JSON Serialization

> **Category:** Data | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers JSON serialization patterns using System.Text.Json for 3SC widgets.

## Prerequisites

- Basic C# knowledge

---

## Standard Configuration

### Default Options

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

public static class JsonConfig
{
    public static JsonSerializerOptions DefaultOptions { get; } = new()
    {
        // Pretty print for readability
        WriteIndented = true,
        
        // Handle property name casing
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        
        // Handle null values
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        
        // Handle numbers in strings
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        
        // Handle enums
        Converters = { new JsonStringEnumConverter() }
    };
    
    public static JsonSerializerOptions CompactOptions { get; } = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
```

---

## Model Attributes

### Basic Model

```csharp
public class WidgetSettings
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = "My Widget";
    
    [JsonPropertyName("refreshInterval")]
    public int RefreshIntervalSeconds { get; set; } = 60;
    
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Ignored in serialization
    [JsonIgnore]
    public bool IsDirty { get; set; }
    
    // Only serialize if not null
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    // Only serialize if not default
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("priority")]
    public int Priority { get; set; }
}
```

### Nested Objects

```csharp
public class AppSettings
{
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;
    
    [JsonPropertyName("window")]
    public WindowSettings Window { get; set; } = new();
    
    [JsonPropertyName("appearance")]
    public AppearanceSettings Appearance { get; set; } = new();
    
    [JsonPropertyName("features")]
    public Dictionary<string, bool> Features { get; set; } = new();
    
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();
}

public class WindowSettings
{
    [JsonPropertyName("x")]
    public double X { get; set; }
    
    [JsonPropertyName("y")]
    public double Y { get; set; }
    
    [JsonPropertyName("width")]
    public double Width { get; set; } = 300;
    
    [JsonPropertyName("height")]
    public double Height { get; set; } = 200;
}

public class AppearanceSettings
{
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "dark";
    
    [JsonPropertyName("opacity")]
    public double Opacity { get; set; } = 1.0;
    
    [JsonPropertyName("accentColor")]
    public string AccentColor { get; set; } = "#2DD4BF";
}
```

---

## Serialization & Deserialization

### Basic Operations

```csharp
// Serialize to string
var settings = new WidgetSettings { DisplayName = "Clock" };
string json = JsonSerializer.Serialize(settings, JsonConfig.DefaultOptions);

// Deserialize from string
var loaded = JsonSerializer.Deserialize<WidgetSettings>(json, JsonConfig.DefaultOptions);

// Serialize to file
await using var stream = File.Create("settings.json");
await JsonSerializer.SerializeAsync(stream, settings, JsonConfig.DefaultOptions);

// Deserialize from file
await using var readStream = File.OpenRead("settings.json");
var fromFile = await JsonSerializer.DeserializeAsync<WidgetSettings>(readStream, JsonConfig.DefaultOptions);
```

### Safe Deserialization

```csharp
public static T? SafeDeserialize<T>(string json) where T : class
{
    if (string.IsNullOrWhiteSpace(json))
        return null;
    
    try
    {
        return JsonSerializer.Deserialize<T>(json, JsonConfig.DefaultOptions);
    }
    catch (JsonException ex)
    {
        Log.Warning(ex, "Failed to deserialize JSON");
        return null;
    }
}

public static async Task<T?> SafeDeserializeFromFileAsync<T>(string path) where T : class, new()
{
    if (!File.Exists(path))
        return new T();
    
    try
    {
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<T>(json, JsonConfig.DefaultOptions) ?? new T();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to deserialize from file: {Path}", path);
        return new T();
    }
}
```

---

## Custom Converters

### DateTime Converter

```csharp
public class DateTimeConverter : JsonConverter<DateTime>
{
    private readonly string _format;
    
    public DateTimeConverter(string format = "yyyy-MM-ddTHH:mm:ssZ")
    {
        _format = format;
    }
    
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return default;
        
        if (DateTime.TryParse(value, out var result))
            return result;
        
        return DateTime.ParseExact(value, _format, CultureInfo.InvariantCulture);
    }
    
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format, CultureInfo.InvariantCulture));
    }
}
```

### TimeSpan Converter

```csharp
public class TimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return TimeSpan.Zero;
        
        // Support multiple formats
        if (TimeSpan.TryParse(value, out var result))
            return result;
        
        // Support "1h30m" format
        return ParseHumanReadable(value);
    }
    
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(@"hh\:mm\:ss"));
    }
    
    private TimeSpan ParseHumanReadable(string value)
    {
        var total = TimeSpan.Zero;
        var match = Regex.Match(value, @"(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?");
        
        if (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out var hours))
                total += TimeSpan.FromHours(hours);
            if (int.TryParse(match.Groups[2].Value, out var minutes))
                total += TimeSpan.FromMinutes(minutes);
            if (int.TryParse(match.Groups[3].Value, out var seconds))
                total += TimeSpan.FromSeconds(seconds);
        }
        
        return total;
    }
}
```

### Color Converter

```csharp
public class ColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return Colors.Transparent;
        
        try
        {
            return (Color)ColorConverter.ConvertFromString(value);
        }
        catch
        {
            return Colors.Transparent;
        }
    }
    
    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}");
    }
}
```

### Guid Converter

```csharp
public class GuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return string.IsNullOrEmpty(value) ? Guid.Empty : Guid.Parse(value);
    }
    
    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("D"));
    }
}
```

---

## Polymorphic Serialization

### With Type Discriminator

```csharp
[JsonDerivedType(typeof(TextNote), "text")]
[JsonDerivedType(typeof(ChecklistNote), "checklist")]
[JsonDerivedType(typeof(ImageNote), "image")]
public abstract class NoteBase
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class TextNote : NoteBase
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}

public class ChecklistNote : NoteBase
{
    [JsonPropertyName("items")]
    public List<ChecklistItem> Items { get; set; } = new();
}

public class ChecklistItem
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = "";
    
    [JsonPropertyName("isChecked")]
    public bool IsChecked { get; set; }
}

public class ImageNote : NoteBase
{
    [JsonPropertyName("imagePath")]
    public string ImagePath { get; set; } = "";
    
    [JsonPropertyName("caption")]
    public string? Caption { get; set; }
}

// Usage
List<NoteBase> notes = new()
{
    new TextNote { Title = "Meeting Notes", Content = "..." },
    new ChecklistNote { Title = "Todo", Items = new() { new() { Text = "Task 1" } } }
};

var json = JsonSerializer.Serialize(notes, JsonConfig.DefaultOptions);
var loaded = JsonSerializer.Deserialize<List<NoteBase>>(json, JsonConfig.DefaultOptions);
```

---

## JSON Document API

For dynamic JSON handling:

```csharp
// Parse without knowing structure
using var doc = JsonDocument.Parse(json);
var root = doc.RootElement;

// Read properties
if (root.TryGetProperty("name", out var nameElement))
{
    var name = nameElement.GetString();
}

// Enumerate arrays
if (root.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
{
    foreach (var item in items.EnumerateArray())
    {
        Console.WriteLine(item.GetProperty("title").GetString());
    }
}

// Enumerate objects
foreach (var prop in root.EnumerateObject())
{
    Console.WriteLine($"{prop.Name}: {prop.Value}");
}
```

---

## JsonNode API

For modifying JSON:

```csharp
using System.Text.Json.Nodes;

// Parse as mutable
var node = JsonNode.Parse(json);

// Modify values
node!["name"] = "New Name";
node["settings"]!["theme"] = "light";

// Add new properties
node["newProperty"] = 42;

// Remove properties
node.AsObject().Remove("oldProperty");

// Convert back to string
var modified = node.ToJsonString(JsonConfig.DefaultOptions);
```

---

## Migration Patterns

### Version-Based Migration

```csharp
public class MigratableSettings
{
    private const int CurrentVersion = 3;
    
    [JsonPropertyName("version")]
    public int Version { get; set; } = CurrentVersion;
    
    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }
    
    public T? GetData<T>() where T : class
    {
        return JsonSerializer.Deserialize<T>(Data.GetRawText(), JsonConfig.DefaultOptions);
    }
    
    public static MigratableSettings Load(string path)
    {
        var json = File.ReadAllText(path);
        var settings = JsonSerializer.Deserialize<MigratableSettings>(json, JsonConfig.DefaultOptions);
        
        if (settings == null)
            return new MigratableSettings();
        
        // Migrate if needed
        while (settings.Version < CurrentVersion)
        {
            settings = Migrate(settings);
        }
        
        return settings;
    }
    
    private static MigratableSettings Migrate(MigratableSettings settings)
    {
        return settings.Version switch
        {
            1 => MigrateV1ToV2(settings),
            2 => MigrateV2ToV3(settings),
            _ => settings
        };
    }
    
    private static MigratableSettings MigrateV1ToV2(MigratableSettings settings)
    {
        // Migration logic
        var node = JsonNode.Parse(settings.Data.GetRawText())!;
        
        // Rename property
        if (node["oldName"] != null)
        {
            node["newName"] = node["oldName"];
            node.AsObject().Remove("oldName");
        }
        
        settings.Data = JsonSerializer.Deserialize<JsonElement>(node.ToJsonString());
        settings.Version = 2;
        return settings;
    }
    
    private static MigratableSettings MigrateV2ToV3(MigratableSettings settings)
    {
        // V2 to V3 migration
        settings.Version = 3;
        return settings;
    }
}
```

---

## Best Practices

### 1. Always Specify Property Names

```csharp
// Good - explicit names
[JsonPropertyName("userName")]
public string UserName { get; set; }

// Risky - depends on naming policy
public string UserName { get; set; }
```

### 2. Use Nullable Types

```csharp
// Good - handles missing values
[JsonPropertyName("description")]
public string? Description { get; set; }

// Risky - might throw
[JsonPropertyName("description")]
public string Description { get; set; }
```

### 3. Provide Defaults

```csharp
public class Settings
{
    [JsonPropertyName("timeout")]
    public int TimeoutSeconds { get; set; } = 30;
    
    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; } = 3;
}
```

---

## Common Mistakes

### ❌ Not Handling Null

```csharp
// Crashes if json is null
var obj = JsonSerializer.Deserialize<MyClass>(json);
obj.Property = "value"; // NullReferenceException!
```

### ❌ Inconsistent Naming

```csharp
// File has camelCase, class has PascalCase
// Without PropertyNameCaseInsensitive, this fails
public string UserName { get; set; }
// JSON: {"userName": "value"}
```

---

## Related Skills

- [settings-management.md](settings-management.md) - Settings patterns
- [data-persistence.md](data-persistence.md) - File storage

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added polymorphism, migration |
| 1.0.0 | 2025-06-01 | Initial version |
