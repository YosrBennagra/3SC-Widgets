# Localization

> **Category:** Advanced | **Priority:** 🟢 Optional
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

Implement multi-language support in widgets using resource files and culture-aware formatting.

---

## Resource File Setup

### Create Resource Files

```
MyWidget/
├── Resources/
│   ├── Strings.resx           (Default/English)
│   ├── Strings.es.resx        (Spanish)
│   ├── Strings.de.resx        (German)
│   ├── Strings.fr.resx        (French)
│   └── Strings.ja.resx        (Japanese)
```

### Strings.resx Content

| Name | Value |
|------|-------|
| AppTitle | My Widget |
| Settings | Settings |
| Save | Save |
| Cancel | Cancel |
| Loading | Loading... |
| Error_Generic | An error occurred |
| Error_Network | Network connection failed |

### Strings.es.resx Content

| Name | Value |
|------|-------|
| AppTitle | Mi Widget |
| Settings | Configuración |
| Save | Guardar |
| Cancel | Cancelar |
| Loading | Cargando... |
| Error_Generic | Ocurrió un error |
| Error_Network | Conexión de red fallida |

---

## Localization Service

```csharp
public class LocalizationService
{
    private static ResourceManager? _resourceManager;
    private static CultureInfo _currentCulture = CultureInfo.CurrentUICulture;
    
    public static void Initialize(Type resourceType)
    {
        _resourceManager = new ResourceManager(resourceType);
    }
    
    public static string GetString(string key)
    {
        if (_resourceManager == null)
            return $"[{key}]";
        
        try
        {
            return _resourceManager.GetString(key, _currentCulture) ?? $"[{key}]";
        }
        catch
        {
            return $"[{key}]";
        }
    }
    
    public static string GetString(string key, params object[] args)
    {
        var format = GetString(key);
        try
        {
            return string.Format(_currentCulture, format, args);
        }
        catch
        {
            return format;
        }
    }
    
    public static void SetCulture(string cultureName)
    {
        _currentCulture = new CultureInfo(cultureName);
    }
    
    public static CultureInfo CurrentCulture => _currentCulture;
}

// Shorthand accessor
public static class L
{
    public static string T(string key) => LocalizationService.GetString(key);
    public static string T(string key, params object[] args) => LocalizationService.GetString(key, args);
}
```

---

## XAML Localization Extension

```csharp
[MarkupExtensionReturnType(typeof(string))]
public class LocalizeExtension : MarkupExtension
{
    public string Key { get; set; } = "";
    
    public LocalizeExtension() { }
    public LocalizeExtension(string key) => Key = key;
    
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return LocalizationService.GetString(Key);
    }
}
```

### Usage in XAML

```xml
<Window xmlns:local="clr-namespace:MyWidget">
    <StackPanel>
        <TextBlock Text="{local:Localize AppTitle}"/>
        <Button Content="{local:Localize Save}"/>
        <Button Content="{local:Localize Cancel}"/>
    </StackPanel>
</Window>
```

---

## ViewModel Localization

```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = L.T("AppTitle");
    
    [ObservableProperty]
    private string _statusMessage = "";
    
    public void ShowError(Exception ex)
    {
        StatusMessage = L.T("Error_Generic");
    }
    
    public void ShowProgress(int current, int total)
    {
        // "Processing {0} of {1}..."
        StatusMessage = L.T("Progress_Format", current, total);
    }
}
```

---

## Date/Time Formatting

```csharp
public static class DateTimeFormatter
{
    public static string FormatDate(DateTime date)
    {
        return date.ToString("d", LocalizationService.CurrentCulture);
    }
    
    public static string FormatTime(DateTime time)
    {
        return time.ToString("t", LocalizationService.CurrentCulture);
    }
    
    public static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("g", LocalizationService.CurrentCulture);
    }
    
    public static string FormatRelative(DateTime dateTime)
    {
        var diff = DateTime.Now - dateTime;
        
        if (diff.TotalMinutes < 1)
            return L.T("Time_JustNow");
        if (diff.TotalMinutes < 60)
            return L.T("Time_MinutesAgo", (int)diff.TotalMinutes);
        if (diff.TotalHours < 24)
            return L.T("Time_HoursAgo", (int)diff.TotalHours);
        if (diff.TotalDays < 7)
            return L.T("Time_DaysAgo", (int)diff.TotalDays);
        
        return FormatDate(dateTime);
    }
}
```

---

## Number Formatting

```csharp
public static class NumberFormatter
{
    public static string FormatNumber(double value, int decimals = 0)
    {
        return value.ToString($"N{decimals}", LocalizationService.CurrentCulture);
    }
    
    public static string FormatPercent(double value)
    {
        return value.ToString("P0", LocalizationService.CurrentCulture);
    }
    
    public static string FormatCurrency(decimal value)
    {
        return value.ToString("C", LocalizationService.CurrentCulture);
    }
    
    public static string FormatFileSize(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double size = bytes;
        int unit = 0;
        
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }
        
        return $"{size:F1} {units[unit]}";
    }
}
```

---

## Language Selection

```csharp
public class LanguageOption
{
    public string Code { get; init; } = "";
    public string DisplayName { get; init; } = "";
}

public partial class SettingsViewModel : ObservableObject
{
    public List<LanguageOption> AvailableLanguages { get; } = new()
    {
        new() { Code = "en", DisplayName = "English" },
        new() { Code = "es", DisplayName = "Español" },
        new() { Code = "de", DisplayName = "Deutsch" },
        new() { Code = "fr", DisplayName = "Français" },
        new() { Code = "ja", DisplayName = "日本語" }
    };
    
    [ObservableProperty]
    private LanguageOption _selectedLanguage;
    
    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        LocalizationService.SetCulture(value.Code);
        _settings.Current.LanguageCode = value.Code;
        // Note: Full UI update may require restart
    }
}
```

---

## Pluralization

```csharp
public static class Pluralizer
{
    public static string Pluralize(int count, string singular, string plural)
    {
        return count == 1 ? singular : plural;
    }
    
    public static string FormatCount(int count, string singularKey, string pluralKey)
    {
        var template = count == 1 ? L.T(singularKey) : L.T(pluralKey);
        return string.Format(template, count);
    }
}

// Resource strings:
// Items_Singular = "{0} item"
// Items_Plural = "{0} items"

// Usage:
var text = Pluralizer.FormatCount(count, "Items_Singular", "Items_Plural");
```

---

## RTL Support

```csharp
public static class RtlHelper
{
    private static readonly HashSet<string> RtlLanguages = new()
    {
        "ar", "he", "fa", "ur"
    };
    
    public static FlowDirection GetFlowDirection(string languageCode)
    {
        return RtlLanguages.Contains(languageCode) 
            ? FlowDirection.RightToLeft 
            : FlowDirection.LeftToRight;
    }
}

// Apply in XAML root
<UserControl FlowDirection="{Binding FlowDirection}">
```

---

## Initialization

```csharp
public class WidgetFactory : IWidgetFactory
{
    public IWidget Create()
    {
        // Initialize localization
        LocalizationService.Initialize(typeof(Resources.Strings));
        
        // Load saved language preference
        var settings = LoadSettings();
        if (!string.IsNullOrEmpty(settings.LanguageCode))
        {
            LocalizationService.SetCulture(settings.LanguageCode);
        }
        
        return new MyWidget();
    }
}
```

---

## Best Practices

1. **Never hardcode strings** - Always use resource files
2. **Use format strings** - Support variable substitution
3. **Handle missing keys** - Return key name as fallback
4. **Test with long text** - German text is ~30% longer than English
5. **Consider RTL** - Support right-to-left languages if needed

---

## Related Skills

- [settings-management.md](../data/settings-management.md) - Persist language preference
- [xaml-styling.md](../ui/xaml-styling.md) - UI considerations

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added RTL support |
| 1.0.0 | 2025-06-01 | Initial version |
