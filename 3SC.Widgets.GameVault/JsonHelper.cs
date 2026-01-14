using System;
using System.Text.Json;

namespace _3SC.Widgets.GameVault;

public static class JsonHelper
{
    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
    }

    public static T? Deserialize<T>(string? json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json)) return default;
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }
}
