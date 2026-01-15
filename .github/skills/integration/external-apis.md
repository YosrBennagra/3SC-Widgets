# External APIs

> **Category:** Integration | **Priority:** 🟢 Optional
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers integrating external REST APIs and web services into widgets, including authentication, error handling, and caching.

---

## HTTP Client Setup

```csharp
public class ApiClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly ILogger _log;
    private readonly string _baseUrl;
    
    public ApiClient(string baseUrl, ILogger log)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _log = log.ForContext<ApiClient>();
        
        _http = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("3SC-Widget/1.0");
    }
    
    public void SetApiKey(string apiKey)
    {
        _http.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", apiKey);
    }
    
    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        try
        {
            _log.Debug("GET {Endpoint}", endpoint);
            
            var response = await _http.GetAsync(endpoint, ct);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _log.Warning(ex, "API request failed: GET {Endpoint}", endpoint);
            throw new ApiException("Request failed", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _log.Warning("API request timed out: GET {Endpoint}", endpoint);
            throw new ApiException("Request timed out", ex);
        }
    }
    
    public async Task<T?> PostAsync<T>(string endpoint, object data, CancellationToken ct = default)
    {
        try
        {
            _log.Debug("POST {Endpoint}", endpoint);
            
            var json = JsonSerializer.Serialize(data, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _http.PostAsync(endpoint, content, ct);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<T>(responseContent, JsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _log.Warning(ex, "API request failed: POST {Endpoint}", endpoint);
            throw new ApiException("Request failed", ex);
        }
    }
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
    
    public void Dispose()
    {
        _http.Dispose();
    }
}

public class ApiException : Exception
{
    public ApiException(string message, Exception? inner = null) 
        : base(message, inner) { }
}
```

---

## Response Caching

```csharp
public class CachedApiClient : IDisposable
{
    private readonly ApiClient _api;
    private readonly ILogger _log;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(5);
    
    public CachedApiClient(string baseUrl, ILogger log)
    {
        _api = new ApiClient(baseUrl, log);
        _log = log;
    }
    
    public async Task<T?> GetCachedAsync<T>(
        string endpoint, 
        TimeSpan? ttl = null,
        CancellationToken ct = default)
    {
        var cacheKey = $"GET:{endpoint}";
        
        // Check cache
        if (_cache.TryGetValue(cacheKey, out var entry) && !entry.IsExpired)
        {
            _log.Debug("Cache hit: {Key}", cacheKey);
            return (T?)entry.Value;
        }
        
        // Fetch from API
        _log.Debug("Cache miss: {Key}", cacheKey);
        var result = await _api.GetAsync<T>(endpoint, ct);
        
        // Cache result
        _cache[cacheKey] = new CacheEntry
        {
            Value = result,
            ExpiresAt = DateTime.UtcNow + (ttl ?? _defaultTtl)
        };
        
        return result;
    }
    
    public void InvalidateCache(string? endpointPattern = null)
    {
        if (endpointPattern == null)
        {
            _cache.Clear();
            return;
        }
        
        var keysToRemove = _cache.Keys
            .Where(k => k.Contains(endpointPattern))
            .ToList();
        
        foreach (var key in keysToRemove)
        {
            _cache.TryRemove(key, out _);
        }
    }
    
    public void Dispose()
    {
        _api.Dispose();
    }
    
    private class CacheEntry
    {
        public object? Value { get; init; }
        public DateTime ExpiresAt { get; init; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }
}
```

---

## Retry with Polly Pattern

```csharp
public class ResilientApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger _log;
    
    public ResilientApiClient(string baseUrl, ILogger log)
    {
        _log = log;
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }
    
    public async Task<T?> GetWithRetryAsync<T>(
        string endpoint,
        int maxRetries = 3,
        CancellationToken ct = default)
    {
        var delay = TimeSpan.FromSeconds(1);
        Exception? lastException = null;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _http.GetAsync(endpoint, ct);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<T>(content);
            }
            catch (HttpRequestException ex) when (IsTransient(ex))
            {
                lastException = ex;
                _log.Warning("Attempt {Attempt}/{Max} failed, retrying in {Delay}s", 
                    attempt, maxRetries, delay.TotalSeconds);
                
                if (attempt < maxRetries)
                {
                    await Task.Delay(delay, ct);
                    delay *= 2; // Exponential backoff
                }
            }
        }
        
        throw new ApiException($"Failed after {maxRetries} attempts", lastException);
    }
    
    private static bool IsTransient(HttpRequestException ex)
    {
        return ex.StatusCode is 
            HttpStatusCode.RequestTimeout or
            HttpStatusCode.ServiceUnavailable or
            HttpStatusCode.GatewayTimeout or
            null; // Network error
    }
}
```

---

## API Response Models

```csharp
// Generic wrapper for API responses
public class ApiResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }
    
    [JsonPropertyName("error")]
    public ApiError? Error { get; set; }
    
    [JsonPropertyName("meta")]
    public ApiMeta? Meta { get; set; }
    
    public bool IsSuccess => Error == null;
}

public class ApiError
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}

public class ApiMeta
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
    
    [JsonPropertyName("page")]
    public int Page { get; set; }
    
    [JsonPropertyName("perPage")]
    public int PerPage { get; set; }
}
```

---

## Usage in ViewModel

```csharp
public partial class WeatherViewModel : ObservableObject
{
    private readonly CachedApiClient _api;
    private readonly ILogger _log;
    private CancellationTokenSource? _cts;
    
    [ObservableProperty]
    private WeatherData? _weather;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _error;
    
    public WeatherViewModel(ILogger log)
    {
        _log = log;
        _api = new CachedApiClient("https://api.weather.example.com", log);
    }
    
    [RelayCommand]
    private async Task LoadWeatherAsync(string city)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        
        IsLoading = true;
        Error = null;
        
        try
        {
            Weather = await _api.GetCachedAsync<WeatherData>(
                $"/weather/{Uri.EscapeDataString(city)}",
                TimeSpan.FromMinutes(15),
                _cts.Token);
        }
        catch (ApiException ex)
        {
            _log.Warning(ex, "Failed to load weather for {City}", city);
            Error = "Unable to load weather data";
        }
        catch (OperationCanceledException)
        {
            // Cancelled, ignore
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

---

## Best Practices

1. **Use HttpClient correctly** - Single instance, dispose properly
2. **Handle timeouts** - Set reasonable timeout values
3. **Implement caching** - Reduce API calls
4. **Add retry logic** - Handle transient failures
5. **Validate responses** - Check for errors before using data

---

## Related Skills

- [async-patterns.md](../performance/async-patterns.md) - Async patterns
- [error-handling.md](../quality/error-handling.md) - Error handling

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added caching, retry |
| 1.0.0 | 2025-06-01 | Initial version |
