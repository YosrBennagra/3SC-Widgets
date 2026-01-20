using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Threading;

namespace ThreeSC.Widgets.Pomodoro
{
    public partial class PomodoroViewModel : ObservableObject, IDisposable
    {
        private static readonly ILogger Logger = Log.ForContext<PomodoroViewModel>();
        private readonly DispatcherTimer _timer;
        private readonly string _settingsPath;

        // Timer constants
        private const int WORK_DURATION = 25 * 60; // 25 minutes in seconds
        private const int BREAK_DURATION = 5 * 60; // 5 minutes in seconds
        private const int TREE_GROWTH_STAGES = 10; // Number of growth stages

        // Timer state
        [ObservableProperty]
        private int _timeRemaining;

        [ObservableProperty]
        private bool _isRunning;

        [ObservableProperty]
        private bool _isWorkSession;

        [ObservableProperty]
        private string _timerDisplay = "25:00";

        [ObservableProperty]
        private string _sessionTypeText = "Focus Time";

        // Tree growth
        [ObservableProperty]
        private int _treeGrowthStage; // 0 = seed, 10 = full tree

        [ObservableProperty]
        private bool _treeIsDead;

        [ObservableProperty]
        private double _treeOpacity = 1.0;

        // Statistics
        [ObservableProperty]
        private int _totalTreesPlanted;

        [ObservableProperty]
        private double _totalHoursFocused;

        [ObservableProperty]
        private int _currentStreak;

        [ObservableProperty]
        private int _longestStreak;

        [ObservableProperty]
        private DateTime _lastSessionDate;

        [ObservableProperty]
        private int _todaySessions;

        // Progress for UI
        [ObservableProperty]
        private double _progressPercentage;

        private int _sessionStartTime;
        private int _completedSessionsToday;

        public PomodoroViewModel()
        {
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "3SC", "Widgets", "Pomodoro", "settings.json");

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;

            // Initialize to work session
            _isWorkSession = true;
            _timeRemaining = WORK_DURATION;
            _sessionStartTime = WORK_DURATION;
            UpdateTimerDisplay();
        }

        [RelayCommand]
        private void StartTimer()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                _timer.Start();
                _sessionStartTime = TimeRemaining;
                Logger.Information("Timer started for {Type} session", IsWorkSession ? "work" : "break");
            }
        }

        [RelayCommand]
        private void PauseTimer()
        {
            if (IsRunning)
            {
                IsRunning = false;
                _timer.Stop();
                Logger.Information("Timer paused");
            }
        }

        [RelayCommand]
        private void StopTimer()
        {
            if (IsRunning || TimeRemaining < _sessionStartTime)
            {
                // User stopped early - kill the tree if it was a work session
                if (IsWorkSession && TreeGrowthStage > 0)
                {
                    KillTree();
                    Logger.Information("Tree died - user stopped timer early");
                }

                ResetTimer();
                Logger.Information("Timer stopped");
            }
        }

        [RelayCommand]
        private void ResetTimer()
        {
            IsRunning = false;
            _timer.Stop();
            TimeRemaining = IsWorkSession ? WORK_DURATION : BREAK_DURATION;
            _sessionStartTime = TimeRemaining;
            UpdateTimerDisplay();
            ProgressPercentage = 0;
        }

        [RelayCommand]
        private void ToggleSessionType()
        {
            IsWorkSession = !IsWorkSession;
            ResetTimer();
            SessionTypeText = IsWorkSession ? "Focus Time" : "Break Time";
            Logger.Information("Switched to {Type} session", IsWorkSession ? "work" : "break");
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            TimeRemaining--;
            UpdateTimerDisplay();

            // Update progress
            double elapsed = _sessionStartTime - TimeRemaining;
            ProgressPercentage = (elapsed / _sessionStartTime) * 100;

            // Grow tree during work sessions
            if (IsWorkSession)
            {
                UpdateTreeGrowth();
            }

            // Session complete
            if (TimeRemaining <= 0)
            {
                CompleteSession();
            }
        }

        private void UpdateTimerDisplay()
        {
            int minutes = TimeRemaining / 60;
            int seconds = TimeRemaining % 60;
            TimerDisplay = $"{minutes:D2}:{seconds:D2}";
        }

        private void UpdateTreeGrowth()
        {
            if (TreeIsDead) return;

            // Calculate expected growth stage based on progress
            int expectedStage = (int)((ProgressPercentage / 100.0) * TREE_GROWTH_STAGES);
            
            if (expectedStage > TreeGrowthStage && expectedStage <= TREE_GROWTH_STAGES)
            {
                TreeGrowthStage = expectedStage;
                Logger.Debug("Tree grew to stage {Stage}", TreeGrowthStage);
            }
        }

        private void CompleteSession()
        {
            _timer.Stop();
            IsRunning = false;

            if (IsWorkSession)
            {
                // Successfully completed a work session!
                TotalTreesPlanted++;
                TotalHoursFocused += WORK_DURATION / 3600.0;
                UpdateStreak();
                
                Logger.Information("Work session completed! Total trees: {Trees}", TotalTreesPlanted);

                // Reset tree for next session
                TreeGrowthStage = 0;
                TreeIsDead = false;
                TreeOpacity = 1.0;

                // Switch to break
                IsWorkSession = false;
                SessionTypeText = "Break Time";
            }
            else
            {
                // Break completed, switch to work
                IsWorkSession = true;
                SessionTypeText = "Focus Time";
            }

            ResetTimer();
            SaveSettings();
        }

        private void KillTree()
        {
            TreeIsDead = true;
            TreeOpacity = 0.3;
            
            // Reset streak if they killed a tree
            CurrentStreak = 0;
        }

        private void UpdateStreak()
        {
            DateTime today = DateTime.Today;

            if (LastSessionDate == default || LastSessionDate.Date == today)
            {
                // First session today or continuing today's sessions
                if (LastSessionDate.Date != today)
                {
                    TodaySessions = 1;
                }
                else
                {
                    TodaySessions++;
                }

                CurrentStreak++;
                LastSessionDate = DateTime.Now;
            }
            else if (LastSessionDate.Date == today.AddDays(-1))
            {
                // Continuing streak from yesterday
                CurrentStreak++;
                TodaySessions = 1;
                LastSessionDate = DateTime.Now;
            }
            else
            {
                // Streak broken
                CurrentStreak = 1;
                TodaySessions = 1;
                LastSessionDate = DateTime.Now;
            }

            if (CurrentStreak > LongestStreak)
            {
                LongestStreak = CurrentStreak;
            }
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<PomodoroSettings>(json);

                    if (settings != null)
                    {
                        TotalTreesPlanted = settings.TotalTreesPlanted;
                        TotalHoursFocused = settings.TotalHoursFocused;
                        CurrentStreak = settings.CurrentStreak;
                        LongestStreak = settings.LongestStreak;
                        LastSessionDate = settings.LastSessionDate;

                        Logger.Information("Settings loaded: {Trees} trees, {Hours:F1} hours", 
                            TotalTreesPlanted, TotalHoursFocused);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load settings");
            }
        }

        public void SaveSettings()
        {
            try
            {
                var settings = new PomodoroSettings
                {
                    TotalTreesPlanted = TotalTreesPlanted,
                    TotalHoursFocused = TotalHoursFocused,
                    CurrentStreak = CurrentStreak,
                    LongestStreak = LongestStreak,
                    LastSessionDate = LastSessionDate
                };

                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);

                Logger.Information("Settings saved");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save settings");
            }
        }

        public void Dispose()
        {
            _timer?.Stop();
            SaveSettings();
        }
    }

    public class PomodoroSettings
    {
        public int TotalTreesPlanted { get; set; }
        public double TotalHoursFocused { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime LastSessionDate { get; set; }
    }
}
