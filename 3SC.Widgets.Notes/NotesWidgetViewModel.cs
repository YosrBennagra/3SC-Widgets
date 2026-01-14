using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace _3SC.Widgets.Notes;

public partial class NotesWidgetViewModel : ObservableObject
{
    private readonly ILogger _logger = Log.ForContext<NotesWidgetViewModel>();
    private CancellationTokenSource? _autoSaveCts;
    private const int AutoSaveDelayMs = 1000; // 1 second after typing stops
    private static readonly char[] WordSplitSeparators = new[] { ' ', '\n', '\r', '\t' };

    [ObservableProperty]
    private string _noteText = "";

    [ObservableProperty]
    private int _characterCount;

    [ObservableProperty]
    private int _wordCount;

    [ObservableProperty]
    private string _lastSaved = "Not saved yet";

    [ObservableProperty]
    private bool _isSaving;

    public NotesWidgetViewModel(bool loadFromDisk = false)
    {
        if (loadFromDisk)
        {
            LoadNote();
        }
    }

    #region Lifecycle Methods

    /// <summary>
    /// Initialize the widget - load saved note.
    /// </summary>
    public void OnInitialize()
    {
        _logger.Debug("Initializing Notes widget");
        LoadNote();
    }

    /// <summary>
    /// Cleanup when widget is disposed.
    /// </summary>
    public void OnDispose()
    {
        _logger.Debug("Disposing Notes widget");
        _autoSaveCts?.Cancel();
        _autoSaveCts?.Dispose();
    }

    #endregion

    partial void OnNoteTextChanged(string value)
    {
        UpdateCounts();
        ScheduleAutoSave();
    }

    private void UpdateCounts()
    {
        var text = NoteText ?? string.Empty;
        CharacterCount = text.Length;

        // Count words (split by whitespace, filter empty)
        var words = text.Split(WordSplitSeparators, StringSplitOptions.RemoveEmptyEntries);
        WordCount = words.Length;
    }

    private void ScheduleAutoSave()
    {
        // Cancel and dispose previous auto-save
        _autoSaveCts?.Cancel();
        _autoSaveCts?.Dispose();
        _autoSaveCts = new CancellationTokenSource();

        var token = _autoSaveCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(AutoSaveDelayMs, token);
                if (!token.IsCancellationRequested)
                {
                    await SaveNoteAsync();
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when typing continues
            }
        }, token);
    }

    [RelayCommand]
    private async Task SaveNoteAsync()
    {
        IsSaving = true;
        try
        {
            // Save to JSON file
            var notesFile = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "3SC", "Widgets", "notes", "notes.json");

            var directory = System.IO.Path.GetDirectoryName(notesFile);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            var data = new
            {
                Text = NoteText,
                LastSaved = DateTime.Now
            };

            await System.IO.File.WriteAllTextAsync(notesFile,
                System.Text.Json.JsonSerializer.Serialize(data));

            LastSaved = $"Saved at {DateTime.Now:HH:mm:ss}";
            _logger.Debug("Note saved successfully");
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to save note");
            LastSaved = "Save failed";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void LoadNote()
    {
        try
        {
            var notesFile = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "3SC", "Widgets", "notes", "notes.json");

            if (System.IO.File.Exists(notesFile))
            {
                var json = System.IO.File.ReadAllText(notesFile);
                var doc = System.Text.Json.JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("Text", out var textElement))
                {
                    NoteText = textElement.GetString() ?? "";
                }

                if (doc.RootElement.TryGetProperty(nameof(LastSaved), out var timeElement) &&
                    DateTime.TryParse(timeElement.GetString(), System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.RoundtripKind, out var lastSavedTime))
                {
                    LastSaved = $"Last saved {lastSavedTime:MMM d, HH:mm}";
                }

                _logger.Debug("Note loaded successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to load note");
        }
    }

    [RelayCommand]
    private void ClearNote() => NoteText = "";
}
