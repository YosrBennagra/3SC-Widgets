using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _3SC.Widgets.LogoSizeTester;

public partial class LogoSizeTesterViewModel : ObservableObject
{
    private static readonly ILogger Logger = Log.ForContext<LogoSizeTesterViewModel>();

    [ObservableProperty]
    private BitmapImage? _logoImage;

    [ObservableProperty]
    private bool _hasLogo;

    [ObservableProperty]
    private string _fileName = "No logo loaded";

    [ObservableProperty]
    private Brush _backgroundColor = new SolidColorBrush(Colors.White);

    [ObservableProperty]
    private bool _isBackgroundDark;

    [ObservableProperty]
    private string _contrastRatio = "N/A";

    [ObservableProperty]
    private string _contrastRating = "";

    // Size information
    [ObservableProperty]
    private string _faviconSize = "16 × 16 px";

    [ObservableProperty]
    private string _mobileSize = "64 × 64 px";

    [ObservableProperty]
    private string _desktopSize = "256 × 256 px";

    [ObservableProperty]
    private string _billboardSize = "512 × 512 px";

    [ObservableProperty]
    private string _minimumLegibleSize = "Calculate first";

    public LogoSizeTesterViewModel()
    {
        IsBackgroundDark = false;
    }

    [RelayCommand]
    private void UploadLogo()
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Logo Image",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadLogo(openFileDialog.FileName);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to upload logo");
            MessageBox.Show($"Failed to load image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadLogo(string filePath)
    {
        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(filePath);
            bitmap.EndInit();
            bitmap.Freeze();

            LogoImage = bitmap;
            HasLogo = true;
            FileName = Path.GetFileName(filePath);

            Logger.Information("Logo loaded: {FileName}, Size: {Width}x{Height}",
                FileName, bitmap.PixelWidth, bitmap.PixelHeight);

            CalculateMinimumLegibleSize();
            UpdateContrast();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load logo from {FilePath}", filePath);
            throw;
        }
    }

    [RelayCommand]
    private void ChangeBackground(string color)
    {
        Color newColor;
        switch (color)
        {
            case "White":
                newColor = Colors.White;
                IsBackgroundDark = false;
                break;
            case "Light Gray":
                newColor = Color.FromRgb(240, 240, 240);
                IsBackgroundDark = false;
                break;
            case "Gray":
                newColor = Color.FromRgb(128, 128, 128);
                IsBackgroundDark = false;
                break;
            case "Dark Gray":
                newColor = Color.FromRgb(64, 64, 64);
                IsBackgroundDark = true;
                break;
            case "Black":
                newColor = Colors.Black;
                IsBackgroundDark = true;
                break;
            case "Blue":
                newColor = Color.FromRgb(41, 128, 185);
                IsBackgroundDark = true;
                break;
            case "Green":
                newColor = Color.FromRgb(39, 174, 96);
                IsBackgroundDark = true;
                break;
            case "Red":
                newColor = Color.FromRgb(231, 76, 60);
                IsBackgroundDark = true;
                break;
            default:
                newColor = Colors.White;
                IsBackgroundDark = false;
                break;
        }

        BackgroundColor = new SolidColorBrush(newColor);
        UpdateContrast();
        Logger.Information("Background changed to {Color}", color);
    }

    private void UpdateContrast()
    {
        if (!HasLogo || LogoImage == null)
        {
            ContrastRatio = "N/A";
            ContrastRating = "";
            return;
        }

        try
        {
            // Get average logo color
            var avgColor = GetAverageLogoColor();
            var bgColor = ((SolidColorBrush)BackgroundColor).Color;

            // Calculate contrast ratio
            var contrast = CalculateContrastRatio(avgColor, bgColor);
            ContrastRatio = $"{contrast:F2}:1";

            // Rate the contrast
            if (contrast >= 7.0)
                ContrastRating = "✅ Excellent (AAA)";
            else if (contrast >= 4.5)
                ContrastRating = "✓ Good (AA)";
            else if (contrast >= 3.0)
                ContrastRating = "⚠ Fair";
            else
                ContrastRating = "❌ Poor";

            Logger.Debug("Contrast ratio: {Ratio}, Rating: {Rating}", ContrastRatio, ContrastRating);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to calculate contrast");
            ContrastRatio = "Error";
            ContrastRating = "";
        }
    }

    private Color GetAverageLogoColor()
    {
        if (LogoImage == null) return Colors.Gray;

        try
        {
            var formatConvertedBitmap = new FormatConvertedBitmap();
            formatConvertedBitmap.BeginInit();
            formatConvertedBitmap.Source = LogoImage;
            formatConvertedBitmap.DestinationFormat = PixelFormats.Bgra32;
            formatConvertedBitmap.EndInit();

            int stride = formatConvertedBitmap.PixelWidth * 4;
            byte[] pixels = new byte[formatConvertedBitmap.PixelHeight * stride];
            formatConvertedBitmap.CopyPixels(pixels, stride, 0);

            long totalR = 0, totalG = 0, totalB = 0;
            int count = 0;

            // Sample every 10th pixel for performance
            for (int i = 0; i < pixels.Length; i += 40) // 4 bytes * 10
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3];

                // Only count non-transparent pixels
                if (a > 50)
                {
                    totalB += b;
                    totalG += g;
                    totalR += r;
                    count++;
                }
            }

            if (count == 0) return Colors.Gray;

            return Color.FromRgb(
                (byte)(totalR / count),
                (byte)(totalG / count),
                (byte)(totalB / count));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to get average logo color");
            return Colors.Gray;
        }
    }

    private double CalculateContrastRatio(Color color1, Color color2)
    {
        double l1 = GetRelativeLuminance(color1);
        double l2 = GetRelativeLuminance(color2);

        double lighter = Math.Max(l1, l2);
        double darker = Math.Min(l1, l2);

        return (lighter + 0.05) / (darker + 0.05);
    }

    private double GetRelativeLuminance(Color color)
    {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        r = r <= 0.03928 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
        g = g <= 0.03928 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
        b = b <= 0.03928 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);

        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    private void CalculateMinimumLegibleSize()
    {
        if (!HasLogo || LogoImage == null)
        {
            MinimumLegibleSize = "N/A";
            return;
        }

        // Heuristic: logos with fine details need larger minimum sizes
        int width = LogoImage.PixelWidth;
        int height = LogoImage.PixelHeight;

        // If logo is very wide or tall, it needs more space
        double aspectRatio = (double)width / height;

        int minSize;
        if (aspectRatio > 3 || aspectRatio < 0.33)
        {
            // Very wide or very tall logos
            minSize = 48;
        }
        else if (width > 500 || height > 500)
        {
            // High resolution logos with potentially fine details
            minSize = 32;
        }
        else
        {
            // Standard logos
            minSize = 24;
        }

        MinimumLegibleSize = $"~{minSize} × {minSize} px";
        Logger.Information("Calculated minimum legible size: {Size}", MinimumLegibleSize);
    }

    [RelayCommand]
    private async Task ExportAllSizes()
    {
        if (!HasLogo || LogoImage == null)
        {
            MessageBox.Show("Please upload a logo first.", "No Logo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var folderDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Select Export Location",
                FileName = "Select Folder",
                Filter = "Folder|*.folder"
            };

            if (folderDialog.ShowDialog() == true)
            {
                string exportPath = Path.GetDirectoryName(folderDialog.FileName) ?? "";
                string baseName = Path.GetFileNameWithoutExtension(FileName);

                // Export different sizes
                await Task.Run(() =>
                {
                    ExportSize(16, exportPath, $"{baseName}_favicon.png");
                    ExportSize(64, exportPath, $"{baseName}_mobile.png");
                    ExportSize(256, exportPath, $"{baseName}_desktop.png");
                    ExportSize(512, exportPath, $"{baseName}_billboard.png");
                });

                Logger.Information("Exported all sizes to {Path}", exportPath);
                MessageBox.Show($"Successfully exported 4 sizes to:\n{exportPath}",
                    "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to export sizes");
            MessageBox.Show($"Failed to export: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportSize(int size, string path, string fileName)
    {
        if (LogoImage == null) return;

        var renderBitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        var drawingVisual = new DrawingVisual();

        using (var drawingContext = drawingVisual.RenderOpen())
        {
            drawingContext.DrawImage(LogoImage, new Rect(0, 0, size, size));
        }

        renderBitmap.Render(drawingVisual);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

        string fullPath = Path.Combine(path, fileName);
        using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            encoder.Save(fileStream);
        }

        Logger.Debug("Exported {Size}x{Size} to {FileName}", size, size, fileName);
    }
}
