# Security: Input Validation

> **Category:** Security | **Priority:** 🔴 Critical
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers input validation and sanitization for widget security, preventing injection attacks and data corruption.

---

## Validation Principles

```
┌─────────────────────────────────────────────────────────────┐
│                   Input Validation Flow                      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│   User Input → Validate → Sanitize → Process → Store         │
│                                                              │
│   ✓ Whitelist allowed characters                             │
│   ✓ Validate format and length                               │
│   ✓ Sanitize before storage                                  │
│   ✓ Encode for output context                                │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Input Validator Class

```csharp
public static class InputValidator
{
    private static readonly Regex SafeTextRegex = new(@"^[\w\s\-\.@,]+$", RegexOptions.Compiled);
    private static readonly Regex WidgetKeyRegex = new(@"^[a-z][a-z0-9-]{1,49}$", RegexOptions.Compiled);
    private static readonly Regex UrlRegex = new(@"^https?://[\w\-\.]+(:\d+)?(/[\w\-\./?%&=]*)?$", RegexOptions.Compiled);
    private static readonly Regex PathRegex = new(@"^[\w\-\./\\:]+$", RegexOptions.Compiled);
    
    /// <summary>
    /// Validates general text input.
    /// </summary>
    public static ValidationResult ValidateText(string? input, int maxLength = 500)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Failure("Input cannot be empty");
        
        if (input.Length > maxLength)
            return ValidationResult.Failure($"Input exceeds maximum length of {maxLength}");
        
        if (!SafeTextRegex.IsMatch(input))
            return ValidationResult.Failure("Input contains invalid characters");
        
        return ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates widget key format.
    /// </summary>
    public static ValidationResult ValidateWidgetKey(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return ValidationResult.Failure("Widget key is required");
        
        if (!WidgetKeyRegex.IsMatch(key))
            return ValidationResult.Failure(
                "Widget key must be lowercase, start with letter, and contain only letters, numbers, and hyphens");
        
        return ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates URL format.
    /// </summary>
    public static ValidationResult ValidateUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return ValidationResult.Failure("URL is required");
        
        if (!UrlRegex.IsMatch(url))
            return ValidationResult.Failure("Invalid URL format");
        
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return ValidationResult.Failure("Could not parse URL");
        
        if (uri.Scheme != "http" && uri.Scheme != "https")
            return ValidationResult.Failure("Only HTTP and HTTPS URLs are allowed");
        
        return ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates file path.
    /// </summary>
    public static ValidationResult ValidatePath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return ValidationResult.Failure("Path is required");
        
        // Check for path traversal attempts
        if (path.Contains(".."))
            return ValidationResult.Failure("Path traversal not allowed");
        
        if (!PathRegex.IsMatch(path))
            return ValidationResult.Failure("Path contains invalid characters");
        
        // Validate it's within allowed directories
        var fullPath = Path.GetFullPath(path);
        var allowedBase = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        
        if (!fullPath.StartsWith(allowedBase, StringComparison.OrdinalIgnoreCase))
            return ValidationResult.Failure("Path must be within application data directory");
        
        return ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates numeric range.
    /// </summary>
    public static ValidationResult ValidateRange(int value, int min, int max, string fieldName)
    {
        if (value < min || value > max)
            return ValidationResult.Failure($"{fieldName} must be between {min} and {max}");
        
        return ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates email format.
    /// </summary>
    public static ValidationResult ValidateEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return ValidationResult.Failure("Email is required");
        
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email 
                ? ValidationResult.Success() 
                : ValidationResult.Failure("Invalid email format");
        }
        catch
        {
            return ValidationResult.Failure("Invalid email format");
        }
    }
}

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    private ValidationResult(bool isValid, string? errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }
    
    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(string message) => new(false, message);
    
    public void ThrowIfInvalid()
    {
        if (!IsValid)
            throw new ValidationException(ErrorMessage ?? "Validation failed");
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
```

---

## Sanitization

```csharp
public static class Sanitizer
{
    /// <summary>
    /// Removes potentially dangerous characters from text.
    /// </summary>
    public static string SanitizeText(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        
        // Remove control characters
        var sanitized = new string(input.Where(c => !char.IsControl(c)).ToArray());
        
        // Trim excessive whitespace
        sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();
        
        return sanitized;
    }
    
    /// <summary>
    /// Sanitizes a filename.
    /// </summary>
    public static string SanitizeFilename(string? filename)
    {
        if (string.IsNullOrEmpty(filename))
            return "unnamed";
        
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(filename.Where(c => !invalidChars.Contains(c)).ToArray());
        
        // Prevent reserved names
        var reserved = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "LPT1" };
        if (reserved.Contains(sanitized.ToUpperInvariant()))
            sanitized = "_" + sanitized;
        
        return string.IsNullOrWhiteSpace(sanitized) ? "unnamed" : sanitized;
    }
    
    /// <summary>
    /// Sanitizes HTML/XML to prevent injection.
    /// </summary>
    public static string SanitizeHtml(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        
        return input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
    
    /// <summary>
    /// Sanitizes JSON string values.
    /// </summary>
    public static string SanitizeJsonValue(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        
        return input
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}
```

---

## Usage in ViewModel

```csharp
public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _apiUrl;
    
    [ObservableProperty]
    private string? _validationError;
    
    partial void OnApiUrlChanged(string? value)
    {
        // Validate on change
        var result = InputValidator.ValidateUrl(value);
        if (!result.IsValid)
        {
            ValidationError = result.ErrorMessage;
        }
        else
        {
            ValidationError = null;
        }
    }
    
    [RelayCommand]
    private void Save()
    {
        // Validate all fields before saving
        var errors = new List<string>();
        
        var urlResult = InputValidator.ValidateUrl(ApiUrl);
        if (!urlResult.IsValid)
            errors.Add($"API URL: {urlResult.ErrorMessage}");
        
        var intervalResult = InputValidator.ValidateRange(RefreshInterval, 30, 3600, "Refresh interval");
        if (!intervalResult.IsValid)
            errors.Add(intervalResult.ErrorMessage!);
        
        if (errors.Any())
        {
            ValidationError = string.Join("\n", errors);
            return;
        }
        
        // Sanitize before saving
        _settings.ApiUrl = ApiUrl;
        _settings.DisplayName = Sanitizer.SanitizeText(DisplayName);
        
        _settingsService.Save(_settings);
    }
}
```

---

## File Path Security

```csharp
public class SecureFileService
{
    private readonly string _baseDirectory;
    private readonly ILogger _log;
    
    public SecureFileService(string widgetKey, ILogger log)
    {
        _log = log;
        
        // Restrict to widget's data directory
        _baseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "3SC", "WidgetData", Sanitizer.SanitizeFilename(widgetKey));
        
        Directory.CreateDirectory(_baseDirectory);
    }
    
    public string ReadFile(string relativePath)
    {
        var safePath = GetSecurePath(relativePath);
        
        if (!File.Exists(safePath))
            throw new FileNotFoundException("File not found", relativePath);
        
        _log.Debug("Reading file: {Path}", safePath);
        return File.ReadAllText(safePath);
    }
    
    public void WriteFile(string relativePath, string content)
    {
        var safePath = GetSecurePath(relativePath);
        
        // Ensure directory exists
        var dir = Path.GetDirectoryName(safePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        
        _log.Debug("Writing file: {Path}", safePath);
        File.WriteAllText(safePath, content);
    }
    
    private string GetSecurePath(string relativePath)
    {
        // Sanitize filename components
        var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var sanitizedParts = parts.Select(Sanitizer.SanitizeFilename);
        var sanitizedPath = Path.Combine(sanitizedParts.ToArray());
        
        // Resolve full path
        var fullPath = Path.GetFullPath(Path.Combine(_baseDirectory, sanitizedPath));
        
        // Ensure path is within base directory (prevent traversal)
        if (!fullPath.StartsWith(_baseDirectory, StringComparison.OrdinalIgnoreCase))
        {
            _log.Warning("Blocked path traversal attempt: {Path}", relativePath);
            throw new UnauthorizedAccessException("Path traversal not allowed");
        }
        
        return fullPath;
    }
}
```

---

## API Input Validation

```csharp
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger _log;
    
    public async Task<T?> FetchAsync<T>(string endpoint)
    {
        // Validate endpoint
        var result = InputValidator.ValidateUrl(endpoint);
        result.ThrowIfInvalid();
        
        var response = await _http.GetStringAsync(endpoint);
        
        // Validate response size
        if (response.Length > 10_000_000) // 10MB limit
        {
            throw new InvalidOperationException("Response too large");
        }
        
        // Deserialize with error handling
        try
        {
            return JsonSerializer.Deserialize<T>(response);
        }
        catch (JsonException ex)
        {
            _log.Warning(ex, "Failed to parse API response");
            throw new InvalidOperationException("Invalid API response format");
        }
    }
}
```

---

## Best Practices

1. **Validate all input** - Never trust user input
2. **Whitelist, don't blacklist** - Define allowed patterns
3. **Sanitize for context** - Different escaping for HTML, JSON, paths
4. **Limit sizes** - Prevent resource exhaustion
5. **Log validation failures** - Aid debugging and security auditing

---

## Related Skills

- [error-handling.md](../quality/error-handling.md) - Error handling
- [secure-storage.md](secure-storage.md) - Secure data storage

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added file security |
| 1.0.0 | 2025-06-01 | Initial version |
