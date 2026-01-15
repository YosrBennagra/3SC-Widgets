# Media Handling

> **Category:** Advanced | **Priority:** 🟡 Important
> **Version:** 2.0.0 | **Last Updated:** 2026-01-15

## Overview

This skill covers handling media content in widgets, including images, videos, audio, and streaming.

---

## Image Handling

### Loading Images

```csharp
public class ImageService
{
    private readonly ILogger _log;
    
    public ImageService(ILogger log)
    {
        _log = log;
    }
    
    /// <summary>
    /// Loads an image from file.
    /// </summary>
    public BitmapImage? LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            _log.Warning("Image not found: {Path}", path);
            return null;
        }
        
        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze(); // Important for thread safety
            
            return bitmap;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to load image: {Path}", path);
            return null;
        }
    }
    
    /// <summary>
    /// Loads an image from URL asynchronously.
    /// </summary>
    public async Task<BitmapImage?> LoadFromUrlAsync(string url, CancellationToken ct = default)
    {
        try
        {
            using var http = new HttpClient();
            var data = await http.GetByteArrayAsync(url, ct);
            
            var bitmap = new BitmapImage();
            using (var stream = new MemoryStream(data))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }
            bitmap.Freeze();
            
            return bitmap;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to load image from URL: {Url}", url);
            return null;
        }
    }
    
    /// <summary>
    /// Creates a thumbnail from an image.
    /// </summary>
    public BitmapImage CreateThumbnail(string path, int maxWidth, int maxHeight)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.DecodePixelWidth = maxWidth;
        bitmap.UriSource = new Uri(path, UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();
        
        return bitmap;
    }
}
```

### Image Caching

```csharp
public class ImageCache
{
    private readonly ConcurrentDictionary<string, WeakReference<BitmapImage>> _cache = new();
    private readonly ImageService _imageService;
    
    public ImageCache(ImageService imageService)
    {
        _imageService = imageService;
    }
    
    public BitmapImage? GetOrLoad(string path)
    {
        if (_cache.TryGetValue(path, out var weakRef) && 
            weakRef.TryGetTarget(out var cached))
        {
            return cached;
        }
        
        var image = _imageService.LoadFromFile(path);
        if (image != null)
        {
            _cache[path] = new WeakReference<BitmapImage>(image);
        }
        
        return image;
    }
    
    public async Task<BitmapImage?> GetOrLoadAsync(string url, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(url, out var weakRef) && 
            weakRef.TryGetTarget(out var cached))
        {
            return cached;
        }
        
        var image = await _imageService.LoadFromUrlAsync(url, ct);
        if (image != null)
        {
            _cache[url] = new WeakReference<BitmapImage>(image);
        }
        
        return image;
    }
    
    public void Clear()
    {
        _cache.Clear();
    }
}
```

---

## Video Handling

### MediaElement in XAML

```xml
<Grid>
    <MediaElement x:Name="VideoPlayer"
                  LoadedBehavior="Manual"
                  UnloadedBehavior="Stop"
                  MediaOpened="VideoPlayer_MediaOpened"
                  MediaEnded="VideoPlayer_MediaEnded"
                  MediaFailed="VideoPlayer_MediaFailed"/>
    
    <!-- Controls overlay -->
    <StackPanel VerticalAlignment="Bottom"
                Background="#80000000"
                Orientation="Horizontal">
        <Button Content="⏵" Click="Play_Click"/>
        <Button Content="⏸" Click="Pause_Click"/>
        <Button Content="⏹" Click="Stop_Click"/>
        <Slider x:Name="PositionSlider" Width="200"
                ValueChanged="Position_Changed"/>
        <TextBlock x:Name="TimeDisplay" Foreground="White"/>
    </StackPanel>
</Grid>
```

### Video Player Service

```csharp
public class VideoPlayerService : IDisposable
{
    private readonly MediaElement _mediaElement;
    private readonly DispatcherTimer _positionTimer;
    
    public event EventHandler<TimeSpan>? PositionChanged;
    public event EventHandler? MediaEnded;
    
    public TimeSpan Duration { get; private set; }
    public TimeSpan Position => _mediaElement.Position;
    public bool IsPlaying { get; private set; }
    
    public VideoPlayerService(MediaElement mediaElement)
    {
        _mediaElement = mediaElement;
        _mediaElement.MediaOpened += OnMediaOpened;
        _mediaElement.MediaEnded += OnMediaEnded;
        
        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _positionTimer.Tick += OnPositionTick;
    }
    
    public void Load(string path)
    {
        _mediaElement.Source = new Uri(path);
    }
    
    public void Play()
    {
        _mediaElement.Play();
        _positionTimer.Start();
        IsPlaying = true;
    }
    
    public void Pause()
    {
        _mediaElement.Pause();
        _positionTimer.Stop();
        IsPlaying = false;
    }
    
    public void Stop()
    {
        _mediaElement.Stop();
        _positionTimer.Stop();
        IsPlaying = false;
    }
    
    public void Seek(TimeSpan position)
    {
        _mediaElement.Position = position;
    }
    
    public void SetVolume(double volume)
    {
        _mediaElement.Volume = Math.Clamp(volume, 0, 1);
    }
    
    private void OnMediaOpened(object sender, RoutedEventArgs e)
    {
        Duration = _mediaElement.NaturalDuration.HasTimeSpan 
            ? _mediaElement.NaturalDuration.TimeSpan 
            : TimeSpan.Zero;
    }
    
    private void OnMediaEnded(object sender, RoutedEventArgs e)
    {
        Stop();
        MediaEnded?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnPositionTick(object? sender, EventArgs e)
    {
        PositionChanged?.Invoke(this, _mediaElement.Position);
    }
    
    public void Dispose()
    {
        _positionTimer.Stop();
        _mediaElement.Stop();
        _mediaElement.Source = null;
    }
}
```

---

## Audio Handling

```csharp
public class AudioService : IDisposable
{
    private readonly MediaPlayer _player;
    private readonly ILogger _log;
    
    public double Volume
    {
        get => _player.Volume;
        set => _player.Volume = Math.Clamp(value, 0, 1);
    }
    
    public AudioService(ILogger log)
    {
        _log = log;
        _player = new MediaPlayer();
        _player.MediaFailed += OnMediaFailed;
    }
    
    public void Play(string path)
    {
        try
        {
            _player.Open(new Uri(path));
            _player.Play();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to play audio: {Path}", path);
        }
    }
    
    public void PlayEmbedded(string resourceName)
    {
        try
        {
            var uri = new Uri($"pack://application:,,,/Assets/{resourceName}");
            _player.Open(uri);
            _player.Play();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to play embedded audio: {Resource}", resourceName);
        }
    }
    
    public void Stop()
    {
        _player.Stop();
    }
    
    private void OnMediaFailed(object? sender, ExceptionEventArgs e)
    {
        _log.Error(e.ErrorException, "Media playback failed");
    }
    
    public void Dispose()
    {
        _player.Stop();
        _player.Close();
    }
}
```

---

## Streaming

```csharp
public class StreamingService
{
    private readonly HttpClient _http;
    private readonly ILogger _log;
    
    public StreamingService(ILogger log)
    {
        _log = log;
        _http = new HttpClient();
    }
    
    /// <summary>
    /// Downloads a file with progress reporting.
    /// </summary>
    public async Task DownloadWithProgressAsync(
        string url,
        string destinationPath,
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        
        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var downloadedBytes = 0L;
        
        await using var contentStream = await response.Content.ReadAsStreamAsync(ct);
        await using var fileStream = File.Create(destinationPath);
        
        var buffer = new byte[8192];
        int bytesRead;
        
        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
            downloadedBytes += bytesRead;
            
            if (totalBytes > 0)
            {
                progress?.Report((double)downloadedBytes / totalBytes * 100);
            }
        }
    }
}
```

---

## Image in XAML

```xml
<!-- Basic image -->
<Image Source="{Binding ImagePath}"/>

<!-- With fallback -->
<Image>
    <Image.Source>
        <BitmapImage UriSource="{Binding ImageUrl}"
                     DecodePixelWidth="200"/>
    </Image.Source>
</Image>

<!-- Async loading with placeholder -->
<Grid>
    <Image x:Name="PlaceholderImage" 
           Source="/Assets/placeholder.png"
           Visibility="{Binding IsLoading, Converter={StaticResource BoolToVis}}"/>
    <Image Source="{Binding LoadedImage}"
           Visibility="{Binding IsLoaded, Converter={StaticResource BoolToVis}}"/>
</Grid>
```

---

## Best Practices

1. **Freeze images** - Call `Freeze()` for thread safety
2. **Use thumbnails** - Don't load full-size images for previews
3. **Cache loaded media** - Avoid reloading the same content
4. **Handle errors** - Media loading can fail
5. **Dispose resources** - Clean up players and streams

---

## Related Skills

- [optimization.md](../performance/optimization.md) - Performance
- [async-patterns.md](../performance/async-patterns.md) - Async loading

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2026-01-15 | Added streaming support |
| 1.0.0 | 2025-06-01 | Initial version |
