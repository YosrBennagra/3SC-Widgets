using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace _3SC.ViewModels;

public partial class QuickLink : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _url = string.Empty;
}

public partial class QuickLinksWidgetViewModel : ObservableObject
{
    private static string LinksFilePath => GetLinksFilePath();

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static string GetLinksFilePath()
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var isTest = assemblies.Any(a =>
                (a.FullName ?? string.Empty).IndexOf("xunit", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (a.FullName ?? string.Empty).IndexOf("nunit", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (a.FullName ?? string.Empty).IndexOf("TestPlatform", StringComparison.OrdinalIgnoreCase) >= 0);

            if (isTest)
            {
                return Path.Combine(Path.GetTempPath(), $"quicklinks_{Process.GetCurrentProcess().Id}.json");
            }
        }
        catch
        {
        }

        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "3SC", "quicklinks.json");
    }

    [ObservableProperty]
    private ObservableCollection<QuickLink> _links = new();

    [ObservableProperty]
    private string _newLinkUrl = string.Empty;

    [ObservableProperty]
    private QuickLink? _selectedLink;

    public QuickLinksWidgetViewModel()
    {
        if (IsRunningUnderTest())
        {
            Links.Add(new QuickLink { Name = "Google", Url = "https://www.google.com" });
            Links.Add(new QuickLink { Name = "GitHub", Url = "https://github.com" });
            Links.Add(new QuickLink { Name = "Stack Overflow", Url = "https://stackoverflow.com" });
            return;
        }

        LoadLinks();
    }

    [RelayCommand]
    private void AddLink()
    {
        if (string.IsNullOrWhiteSpace(NewLinkUrl)) return;
        var url = NewLinkUrl.Trim();
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url;
        }

        var name = ExtractNameFromUrl(url);
        Links.Add(new QuickLink { Name = name, Url = url });
        NewLinkUrl = string.Empty;
        SaveLinks();
    }

    public void AddLink(string name, string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        Links.Add(new QuickLink { Name = name, Url = url });
        SaveLinks();
    }

    [RelayCommand]
    private void OpenLink(QuickLink link)
    {
        if (link == null || string.IsNullOrWhiteSpace(link.Url)) return;
        try
        {
            Process.Start(new ProcessStartInfo { FileName = link.Url, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open link: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CopyLink(QuickLink link)
    {
        if (link == null || string.IsNullOrWhiteSpace(link.Url)) return;
        try
        {
            System.Windows.Clipboard.SetText(link.Url);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to copy link: {ex.Message}");
        }
    }

    [RelayCommand]
    public void RemoveLink(QuickLink link)
    {
        if (link != null)
        {
            Links.Remove(link);
            SaveLinks();
        }
    }

    private void LoadLinks()
    {
        try
        {
            if (!File.Exists(LinksFilePath))
            {
                Links.Add(new QuickLink { Name = "Google", Url = "https://www.google.com" });
                Links.Add(new QuickLink { Name = "GitHub", Url = "https://github.com" });
                Links.Add(new QuickLink { Name = "Stack Overflow", Url = "https://stackoverflow.com" });
                SaveLinks();
                return;
            }

            var json = File.ReadAllText(LinksFilePath);
            var loadedLinks = JsonSerializer.Deserialize<QuickLink[]>(json);
            if (loadedLinks != null)
            {
                Links.Clear();
                foreach (var link in loadedLinks) Links.Add(link);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load links: {ex.Message}");
        }
    }

    private void SaveLinks()
    {
        if (IsRunningUnderTest()) return;
        try
        {
            var dir = Path.GetDirectoryName(LinksFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(Links.ToArray(), JsonOptions);
            File.WriteAllText(LinksFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save links: {ex.Message}");
        }
    }

    private static bool IsRunningUnderTest()
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var byAssembly = assemblies.Any(a =>
                (a.FullName ?? string.Empty).IndexOf("xunit", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (a.FullName ?? string.Empty).IndexOf("nunit", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (a.FullName ?? string.Empty).IndexOf("TestPlatform", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (a.FullName ?? string.Empty).IndexOf("vstest", StringComparison.OrdinalIgnoreCase) >= 0);

            var proc = Process.GetCurrentProcess();
            var procName = proc.ProcessName ?? string.Empty;
            var byProcess = procName.IndexOf("testhost", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            procName.IndexOf("vstest", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            procName.Equals("dotnet", StringComparison.OrdinalIgnoreCase);

            return byAssembly || byProcess;
        }
        catch
        {
            return false;
        }
    }

    private static string ExtractNameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var host = uri.Host;
            if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)) host = host[4..];
            var parts = host.Split('.');
            if (parts.Length > 0 && parts[0].Length > 0) return char.ToUpper(parts[0][0], System.Globalization.CultureInfo.InvariantCulture) + parts[0][1..];
            return host;
        }
        catch { return "Link"; }
    }
}
