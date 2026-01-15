# Error Handling

> **Category:** Quality | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers error handling patterns for widgets, including exception handling, error states, recovery strategies, and user-friendly error presentation.

---

## Error Handling Philosophy

```
┌─────────────────────────────────────────────────────────────┐
│                    Error Handling Layers                     │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│   1. PREVENT → Validation, guards, null checks               │
│                     ↓                                        │
│   2. CATCH   → Try-catch at boundaries                       │
│                     ↓                                        │
│   3. LOG     → Serilog with context                          │
│                     ↓                                        │
│   4. RECOVER → Retry, fallback, graceful degradation         │
│                     ↓                                        │
│   5. INFORM  → User-friendly message                         │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Exception Types

### Custom Widget Exception

```csharp
/// <summary>
/// Base exception for widget-specific errors.
/// </summary>
public class WidgetException : Exception
{
    public string WidgetKey { get; }
    public ErrorSeverity Severity { get; }
    public bool IsRecoverable { get; }
    
    public WidgetException(
        string widgetKey, 
        string message, 
        ErrorSeverity severity = ErrorSeverity.Error,
        bool isRecoverable = true,
        Exception? innerException = null) 
        : base(message, innerException)
    {
        WidgetKey = widgetKey;
        Severity = severity;
        IsRecoverable = isRecoverable;
    }
}

public enum ErrorSeverity
{
    Warning,    // Non-critical, widget continues
    Error,      // Critical, feature disabled
    Fatal       // Widget cannot function
}
```

### Specific Exception Types

```csharp
/// <summary>
/// Thrown when settings cannot be loaded or saved.
/// </summary>
public class SettingsException : WidgetException
{
    public SettingsException(string widgetKey, string message, Exception? inner = null)
        : base(widgetKey, message, ErrorSeverity.Warning, isRecoverable: true, inner)
    {
    }
}

/// <summary>
/// Thrown when external API calls fail.
/// </summary>
public class ApiException : WidgetException
{
    public int? StatusCode { get; }
    public string? Endpoint { get; }
    
    public ApiException(string widgetKey, string endpoint, int? statusCode, string message)
        : base(widgetKey, message, ErrorSeverity.Error, isRecoverable: true)
    {
        StatusCode = statusCode;
        Endpoint = endpoint;
    }
}

/// <summary>
/// Thrown when required resources are missing.
/// </summary>
public class ResourceNotFoundException : WidgetException
{
    public string ResourcePath { get; }
    
    public ResourceNotFoundException(string widgetKey, string resourcePath)
        : base(widgetKey, $"Resource not found: {resourcePath}", ErrorSeverity.Fatal, isRecoverable: false)
    {
        ResourcePath = resourcePath;
    }
}
```

---

## Global Exception Handler

```csharp
public static class GlobalExceptionHandler
{
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(GlobalExceptionHandler));
    
    public static void Initialize()
    {
        // Handle unhandled exceptions in UI thread
        Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        
        // Handle unhandled exceptions in non-UI threads
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        
        // Handle Task exceptions
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }
    
    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled UI exception");
        
        if (e.Exception is WidgetException we && we.IsRecoverable)
        {
            ShowErrorToUser(we);
            e.Handled = true;
        }
        else
        {
            // Fatal error - show critical error UI
            ShowCriticalError(e.Exception);
            e.Handled = true;
        }
    }
    
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        Log.Fatal(exception, "Unhandled domain exception. IsTerminating: {IsTerminating}", e.IsTerminating);
    }
    
    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");
        e.SetObserved(); // Prevent app crash
    }
    
    private static void ShowErrorToUser(WidgetException exception)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show(
                GetUserFriendlyMessage(exception),
                "Widget Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        });
    }
    
    private static void ShowCriticalError(Exception exception)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show(
                "The widget encountered a critical error and needs to restart.\n\n" +
                "Error details have been logged.",
                "Critical Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        });
    }
    
    private static string GetUserFriendlyMessage(WidgetException exception)
    {
        return exception switch
        {
            SettingsException => "Unable to save your settings. They will be reset to defaults.",
            ApiException api => $"Unable to connect to the service. Please check your internet connection.",
            ResourceNotFoundException => "A required resource is missing. Please reinstall the widget.",
            _ => "An unexpected error occurred. Please try again."
        };
    }
}
```

---

## ViewModel Error Handling

### Error State Pattern

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly ILogger _log;
    
    [ObservableProperty]
    private bool _hasError;
    
    [ObservableProperty]
    private string? _errorMessage;
    
    [ObservableProperty]
    private ErrorSeverity _errorSeverity;
    
    [ObservableProperty]
    private bool _isLoading;
    
    public void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }
    
    private void SetError(string message, ErrorSeverity severity = ErrorSeverity.Error)
    {
        HasError = true;
        ErrorMessage = message;
        ErrorSeverity = severity;
    }
    
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        ClearError();
        IsLoading = true;
        
        try
        {
            Data = await _dataService.LoadAsync();
        }
        catch (HttpRequestException ex)
        {
            _log.Warning(ex, "Failed to load data from API");
            SetError("Unable to load data. Check your connection.", ErrorSeverity.Warning);
        }
        catch (JsonException ex)
        {
            _log.Error(ex, "Failed to parse API response");
            SetError("Data format error. Please try again later.");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Unexpected error loading data");
            SetError("An unexpected error occurred.");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### Error Display in XAML

```xml
<!-- Error banner at top of widget -->
<Border x:Name="ErrorBanner"
        Visibility="{Binding HasError, Converter={StaticResource BoolToVisibility}}"
        Background="{Binding ErrorSeverity, Converter={StaticResource SeverityToBrush}}"
        Padding="10,5">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="Auto"/>
        </Grid.ColumnDefinitions>
        
        <!-- Error icon -->
        <TextBlock Grid.Column="0" 
                   Text="⚠" 
                   FontSize="16" 
                   Margin="0,0,8,0"
                   VerticalAlignment="Center"/>
        
        <!-- Error message -->
        <TextBlock Grid.Column="1" 
                   Text="{Binding ErrorMessage}"
                   TextWrapping="Wrap"
                   VerticalAlignment="Center"/>
        
        <!-- Dismiss button -->
        <Button Grid.Column="2"
                Content="✕"
                Command="{Binding ClearErrorCommand}"
                Style="{StaticResource IconButtonStyle}"/>
    </Grid>
</Border>

<!-- Severity to color converter -->
<local:SeverityToBrushConverter x:Key="SeverityToBrush"
    WarningBrush="#FFA500"
    ErrorBrush="#FF4444"
    FatalBrush="#CC0000"/>
```

---

## Result Pattern

```csharp
/// <summary>
/// Represents the result of an operation that may fail.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public Exception? Exception { get; }
    
    private Result(bool isSuccess, T? value, string? error, Exception? exception)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Exception = exception;
    }
    
    public static Result<T> Success(T value) => new(true, value, null, null);
    public static Result<T> Failure(string error) => new(false, default, error, null);
    public static Result<T> Failure(Exception ex) => new(false, default, ex.Message, ex);
    
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess 
            ? Result<TNew>.Success(mapper(Value!)) 
            : Result<TNew>.Failure(Error!);
    }
    
    public T GetValueOrDefault(T defaultValue)
    {
        return IsSuccess ? Value! : defaultValue;
    }
    
    public void Match(Action<T> onSuccess, Action<string> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value!);
        else
            onFailure(Error!);
    }
}

// Usage
public async Task<Result<WeatherData>> GetWeatherAsync(string city)
{
    try
    {
        var data = await _api.FetchWeatherAsync(city);
        return Result<WeatherData>.Success(data);
    }
    catch (HttpRequestException ex)
    {
        _log.Warning(ex, "Weather API request failed for {City}", city);
        return Result<WeatherData>.Failure("Unable to fetch weather data");
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Unexpected error fetching weather");
        return Result<WeatherData>.Failure(ex);
    }
}

// In ViewModel
var result = await _weatherService.GetWeatherAsync("London");
result.Match(
    onSuccess: weather => Weather = weather,
    onFailure: error => SetError(error)
);
```

---

## Retry Pattern

```csharp
public static class RetryPolicy
{
    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        ILogger? log = null)
    {
        var delay = initialDelay ?? TimeSpan.FromSeconds(1);
        Exception? lastException = null;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                lastException = ex;
                log?.Warning("Attempt {Attempt}/{MaxRetries} failed: {Message}", 
                    attempt, maxRetries, ex.Message);
                
                if (attempt < maxRetries)
                {
                    await Task.Delay(delay);
                    delay *= 2; // Exponential backoff
                }
            }
        }
        
        throw new WidgetException(
            "widget", 
            $"Operation failed after {maxRetries} attempts", 
            innerException: lastException);
    }
    
    private static bool IsTransient(Exception ex)
    {
        return ex is HttpRequestException ||
               ex is TimeoutException ||
               ex is TaskCanceledException;
    }
}

// Usage
var data = await RetryPolicy.ExecuteAsync(
    () => _api.FetchDataAsync(),
    maxRetries: 3,
    initialDelay: TimeSpan.FromSeconds(1),
    log: _log);
```

---

## Graceful Degradation

```csharp
public class WeatherWidget
{
    private readonly IWeatherService _weatherService;
    private readonly ICache _cache;
    private readonly ILogger _log;
    
    public async Task<WeatherData> GetWeatherAsync()
    {
        // Try live data
        try
        {
            var data = await _weatherService.GetCurrentWeatherAsync();
            await _cache.SetAsync("weather", data, TimeSpan.FromMinutes(30));
            return data;
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Failed to fetch live weather, trying cache");
        }
        
        // Fallback to cached data
        var cached = await _cache.GetAsync<WeatherData>("weather");
        if (cached != null)
        {
            _log.Information("Using cached weather data");
            return cached;
        }
        
        // Fallback to static defaults
        _log.Warning("No cached data available, using defaults");
        return WeatherData.Default;
    }
}
```

---

## Validation

```csharp
public static class Validator
{
    public static void NotNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
    }
    
    public static void NotNullOrEmpty(string? value, string paramName)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty", paramName);
    }
    
    public static void InRange(int value, int min, int max, string paramName)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(paramName, value, $"Must be between {min} and {max}");
    }
}

// Usage
public void SetRefreshInterval(int seconds)
{
    Validator.InRange(seconds, 5, 3600, nameof(seconds));
    _refreshInterval = seconds;
}
```

---

## Best Practices

1. **Catch at boundaries** - Handle exceptions at service/UI boundaries
2. **Log with context** - Include relevant data in log messages
3. **User-friendly messages** - Never show stack traces to users
4. **Fail gracefully** - Degrade functionality rather than crash
5. **Validate inputs** - Prevent errors before they occur

---

## Related Skills

- [logging.md](logging.md) - Logging errors
- [testing-strategies.md](testing-strategies.md) - Testing error paths

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added Result pattern, retry policy |
| 1.0.0 | 2025-06-01 | Initial version |
