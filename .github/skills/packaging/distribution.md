# Distribution

> **Category:** Packaging | **Priority:** 🟡 Important
> **Version:** 2.1.0 | **Last Updated:** 2026-01-15

## Overview

Widgets are distributed through the **3SC Workshop Portal** with admin/reviewer approval before becoming available for download in the host app.

---

## Production Distribution Workflow

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Create    │───►│  Package    │───►│  Upload to  │───►│   Review    │
│   Widget    │    │  as ZIP     │    │   Portal    │    │  & Approve  │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
                                                               │
                  ┌─────────────┐    ┌─────────────┐           │
                  │  Download   │◄───│  Available  │◄──────────┘
                  │  via 3SC    │    │  in Portal  │
                  └─────────────┘    └─────────────┘
```

### Steps:

1. **Create Widget** - Develop and test locally
2. **Package as ZIP** - Create `.3scwidget` package (ZIP format)
3. **Upload to Portal** - Submit via Workshop Portal website
4. **Review & Approve** - Admins/reviewers verify the widget
5. **Available in Portal** - Approved widgets appear in catalog
6. **Download via 3SC** - Users browse & install from within host app

---

## Local Testing (Development Only)

> ⚠️ **For development/testing only!** Production widgets must go through Workshop Portal.

During development, place widget files directly in the Community folder **without packaging**:

### Manual Testing Installation

```powershell
# 1. Build in Release mode
dotnet build -c Release

# 2. Define target (use widget "key" from manifest.json)
$widgetKey = "my-widget"
$dest = "$env:APPDATA\3SC\Widgets\Community\$widgetKey"

# 3. Create folder
New-Item -ItemType Directory -Force -Path $dest

# 4. Copy files (unzipped/unpackaged)
$source = ".\bin\Release\net8.0-windows"
Copy-Item "$source\*.dll" $dest -Exclude "3SC.Widgets.Contracts.dll"
Copy-Item "$source\manifest.json" $dest

# 5. Restart 3SC host app to detect widget
```

### Minimum Required Files for Testing

```
%APPDATA%\3SC\Widgets\Community\{widget-key}\
├── manifest.json      (REQUIRED)
├── YourWidget.dll     (REQUIRED - main assembly with factory)
└── [dependencies].dll (optional - widget's own dependencies)
```

> 💡 **Note:** Do NOT copy `3SC.Widgets.Contracts.dll` - the host provides this.

---

## Workshop Portal Submission

The Workshop Portal is the **only** official distribution channel for production widgets.

### Submission Process

1. **Create Account** - Register on the Workshop Portal
2. **Upload Package** - Submit your `.3scwidget` (ZIP) file
3. **Await Review** - Admins/reviewers verify the widget
4. **Receive Feedback** - May request changes or approve
5. **Approval** - Widget becomes publicly available
6. **Users Download** - Via 3SC host app widget browser

### Review Criteria

Reviewers verify:
- ✅ Valid `manifest.json` with all required fields
- ✅ Widget loads and functions without errors
- ✅ No malicious code or security vulnerabilities
- ✅ Proper semantic versioning
- ✅ Appropriate content and functionality
- ✅ Metadata accuracy (name, description, icons)

### Step 1: Create Publisher Account

```bash
# Via Workshop Portal CLI (if available)
workshop login
workshop publisher register --name "My Company" --email "dev@example.com"
```

### Step 2: Prepare for Submission

**Required Assets:**

| Asset | Specifications | Purpose |
|-------|----------------|---------|
| icon.png | 128×128 PNG | Store listing |
| preview.png | 800×600 PNG | Preview gallery |
| screenshots/* | 1280×720 PNG | Feature showcase |
| README.md | Markdown | Description |
| LICENSE | Text | Open source license |

### Step 3: Submit Widget

```bash
# Package the widget
./package.ps1 -Configuration Release

# Submit to Workshop
workshop submit ./my-widget-1.0.0.3scwidget \
  --category "Productivity" \
  --tags "clock,time,utility"
```

---

## Manifest Distribution Fields

```json
{
  "widgetKey": "my-widget",
  "displayName": "My Widget",
  "version": "1.2.0",
  
  "distribution": {
    "category": "Productivity",
    "tags": ["utility", "tools"],
    "featured": false,
    
    "listing": {
      "shortDescription": "A brief one-liner",
      "description": "Detailed description with features",
      "screenshots": [
        "Assets/screenshots/main.png",
        "Assets/screenshots/settings.png"
      ],
      "changelog": "CHANGELOG.md"
    },
    
    "pricing": {
      "model": "free",
      "trialDays": 0
    },
    
    "requirements": {
      "minHostVersion": "3.0.0",
      "permissions": ["network", "filesystem"]
    }
  },
  
  "author": {
    "name": "Developer Name",
    "email": "dev@example.com",
    "url": "https://example.com"
  },
  
  "repository": {
    "type": "git",
    "url": "https://github.com/example/my-widget"
  },
  
  "updateUrl": "https://api.example.com/widgets/my-widget/latest"
}
```

---

## Auto-Update System

### Update Check Service

```csharp
/// <summary>
/// Checks for widget updates from distribution channel.
/// </summary>
public class UpdateService
{
    private readonly string _widgetKey;
    private readonly string _currentVersion;
    private readonly HttpClient _http;
    private readonly ILogger _log;
    
    public UpdateService(string widgetKey, string currentVersion, ILogger log)
    {
        _widgetKey = widgetKey;
        _currentVersion = currentVersion;
        _log = log;
        _http = new HttpClient();
    }
    
    public async Task<UpdateInfo?> CheckForUpdateAsync()
    {
        try
        {
            var manifest = await LoadManifestAsync();
            var updateUrl = manifest.UpdateUrl;
            
            if (string.IsNullOrEmpty(updateUrl))
            {
                _log.Debug("No update URL configured");
                return null;
            }
            
            _log.Information("Checking for updates at {Url}", updateUrl);
            
            var response = await _http.GetStringAsync(updateUrl);
            var latest = JsonSerializer.Deserialize<UpdateResponse>(response);
            
            if (latest == null)
                return null;
            
            var current = SemanticVersion.Parse(_currentVersion);
            var latestVersion = SemanticVersion.Parse(latest.Version);
            
            if (latestVersion > current)
            {
                _log.Information("Update available: v{Current} → v{Latest}", 
                    current, latestVersion);
                
                return new UpdateInfo
                {
                    IsAvailable = true,
                    CurrentVersion = _currentVersion,
                    LatestVersion = latest.Version,
                    DownloadUrl = latest.DownloadUrl,
                    ReleaseNotes = latest.ReleaseNotes,
                    IsMandatory = latest.IsMandatory,
                    PublishedAt = latest.PublishedAt
                };
            }
            
            _log.Debug("Widget is up to date");
            return new UpdateInfo { IsAvailable = false, CurrentVersion = _currentVersion };
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Failed to check for updates");
            return null;
        }
    }
    
    private async Task<WidgetManifest> LoadManifestAsync()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(assemblyLocation);
        var manifestPath = Path.Combine(directory!, "manifest.json");
        
        var json = await File.ReadAllTextAsync(manifestPath);
        return JsonSerializer.Deserialize<WidgetManifest>(json)!;
    }
}

public class UpdateInfo
{
    public bool IsAvailable { get; init; }
    public string CurrentVersion { get; init; } = "";
    public string? LatestVersion { get; init; }
    public string? DownloadUrl { get; init; }
    public string? ReleaseNotes { get; init; }
    public bool IsMandatory { get; init; }
    public DateTime? PublishedAt { get; init; }
}

public class UpdateResponse
{
    public string Version { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public string? ReleaseNotes { get; set; }
    public bool IsMandatory { get; set; }
    public DateTime PublishedAt { get; set; }
}
```

### Update API Endpoint Format

```json
{
  "version": "1.3.0",
  "downloadUrl": "https://releases.example.com/my-widget-1.3.0.3scwidget",
  "releaseNotes": "### What's New\n- Feature X\n- Bug fix Y",
  "isMandatory": false,
  "publishedAt": "2025-03-15T10:00:00Z",
  "checksums": {
    "sha256": "abc123..."
  }
}
```

---

## GitHub Releases Distribution

### Release Workflow

```yaml
name: Release Widget

on:
  push:
    tags:
      - 'v*'

jobs:
  release:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.0.x
    
    - name: Get version from tag
      id: version
      run: echo "VERSION=${GITHUB_REF#refs/tags/v}" >> $GITHUB_OUTPUT
    
    - name: Build & Package
      run: |
        dotnet build -c Release
        ./package.ps1 -Configuration Release
    
    - name: Create Release
      uses: softprops/action-gh-release@v1
      with:
        files: "*.3scwidget"
        generate_release_notes: true
        draft: false
        prerelease: ${{ contains(github.ref, '-') }}
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

### GitHub Update URL

```json
{
  "updateUrl": "https://api.github.com/repos/myorg/my-widget/releases/latest"
}
```

### Parsing GitHub Releases

```csharp
public class GitHubUpdateSource : IUpdateSource
{
    private readonly string _owner;
    private readonly string _repo;
    private readonly HttpClient _http;
    
    public GitHubUpdateSource(string owner, string repo)
    {
        _owner = owner;
        _repo = repo;
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("User-Agent", "3SC-Widget-Updater");
    }
    
    public async Task<UpdateInfo?> GetLatestAsync()
    {
        var url = $"https://api.github.com/repos/{_owner}/{_repo}/releases/latest";
        
        var response = await _http.GetStringAsync(url);
        var release = JsonSerializer.Deserialize<GitHubRelease>(response);
        
        if (release == null)
            return null;
        
        // Extract version from tag (v1.2.3 → 1.2.3)
        var version = release.TagName.TrimStart('v');
        
        // Find .3scwidget asset
        var asset = release.Assets
            .FirstOrDefault(a => a.Name.EndsWith(".3scwidget"));
        
        return new UpdateInfo
        {
            IsAvailable = true,
            LatestVersion = version,
            DownloadUrl = asset?.BrowserDownloadUrl,
            ReleaseNotes = release.Body,
            PublishedAt = release.PublishedAt
        };
    }
}

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = "";
    
    [JsonPropertyName("body")]
    public string Body { get; set; } = "";
    
    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }
    
    [JsonPropertyName("assets")]
    public List<GitHubAsset> Assets { get; set; } = new();
}

public class GitHubAsset
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = "";
}
```

---

## Direct Download Page

### download.html Template

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Download My Widget</title>
    <style>
        body {
            font-family: 'Segoe UI', sans-serif;
            max-width: 800px;
            margin: 50px auto;
            padding: 20px;
            background: #1a1a2e;
            color: #eee;
        }
        .download-btn {
            display: inline-block;
            padding: 15px 40px;
            background: #4a9eff;
            color: white;
            text-decoration: none;
            border-radius: 8px;
            font-size: 18px;
            margin: 20px 0;
        }
        .download-btn:hover {
            background: #3a8eef;
        }
        .version-info {
            color: #888;
            font-size: 14px;
        }
        .checksum {
            font-family: monospace;
            background: #2a2a3e;
            padding: 10px;
            border-radius: 4px;
            word-break: break-all;
        }
    </style>
</head>
<body>
    <h1>My Widget</h1>
    <p>A brief description of what the widget does.</p>
    
    <a href="releases/my-widget-1.2.0.3scwidget" class="download-btn">
        ⬇️ Download v1.2.0
    </a>
    
    <p class="version-info">
        Released: March 15, 2025 • Size: 125 KB
    </p>
    
    <h3>Installation</h3>
    <ol>
        <li>Download the .3scwidget file</li>
        <li>Double-click to install, or drag into 3SC window</li>
        <li>The widget will appear in your widget library</li>
    </ol>
    
    <h3>Verify Download</h3>
    <p>SHA256 checksum:</p>
    <div class="checksum">
        a1b2c3d4e5f6789012345678901234567890abcdef...
    </div>
    
    <h3>Requirements</h3>
    <ul>
        <li>3SC v3.0.0 or later</li>
        <li>Windows 10/11</li>
    </ul>
    
    <h3>Changelog</h3>
    <h4>v1.2.0</h4>
    <ul>
        <li>Added dark theme support</li>
        <li>Fixed settings persistence</li>
    </ul>
</body>
</html>
```

---

## Checksum Generation

### Generate Checksums

```powershell
# generate-checksums.ps1

param(
    [Parameter(Mandatory)]
    [string]$PackagePath
)

$sha256 = (Get-FileHash $PackagePath -Algorithm SHA256).Hash.ToLower()
$md5 = (Get-FileHash $PackagePath -Algorithm MD5).Hash.ToLower()

$checksums = @{
    file = (Get-Item $PackagePath).Name
    sha256 = $sha256
    md5 = $md5
}

# Output as JSON
$checksums | ConvertTo-Json | Tee-Object -FilePath "$PackagePath.checksums.json"

Write-Host "SHA256: $sha256"
Write-Host "MD5:    $md5"
```

### Verify Checksums

```csharp
public static class ChecksumVerifier
{
    public static async Task<bool> VerifyAsync(string filePath, string expectedSha256)
    {
        using var sha256 = SHA256.Create();
        await using var stream = File.OpenRead(filePath);
        
        var hash = await sha256.ComputeHashAsync(stream);
        var actual = Convert.ToHexString(hash).ToLowerInvariant();
        
        return actual.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase);
    }
}
```

---

## Best Practices

1. **Sign packages** - Use code signing for production
2. **Provide checksums** - SHA256 for verification
3. **Version URLs** - Include version in download URLs
4. **Graceful updates** - Don't force immediate updates
5. **Changelog** - Document all changes

---

## Related Skills

- [packaging-deployment.md](packaging-deployment.md) - Creating packages
- [versioning.md](versioning.md) - Version management

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added GitHub releases integration |
| 1.0.0 | 2025-06-01 | Initial version |
