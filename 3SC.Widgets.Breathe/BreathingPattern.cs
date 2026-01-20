namespace _3SC.Widgets.Breathe;

public enum BreathPhase
{
    Idle,
    Inhale,
    Hold,
    Exhale,
    Rest
}

public class BreathingPattern
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int InhaleSeconds { get; set; }
    public int HoldSeconds { get; set; }
    public int ExhaleSeconds { get; set; }
    public int RestSeconds { get; set; }
    public string ColorStart { get; set; } = "#4FD1C5";
    public string ColorEnd { get; set; } = "#38B2AC";

    public int TotalCycleSeconds => InhaleSeconds + HoldSeconds + ExhaleSeconds + RestSeconds;
}
