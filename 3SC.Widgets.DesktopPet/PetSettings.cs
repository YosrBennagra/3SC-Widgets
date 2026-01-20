using System.Text.Json.Serialization;

namespace _3SC.Widgets.DesktopPet;

/// <summary>
/// Persistent settings for the Desktop Pet widget.
/// </summary>
public class PetSettings
{
    /// <summary>The pet's name</summary>
    [JsonPropertyName("petName")]
    public string PetName { get; set; } = "Blobby";

    /// <summary>Current happiness level (0-100)</summary>
    [JsonPropertyName("happiness")]
    public double Happiness { get; set; } = 80;

    /// <summary>Current energy level (0-100)</summary>
    [JsonPropertyName("energy")]
    public double Energy { get; set; } = 100;

    /// <summary>Current hunger level (0-100, 100 = full)</summary>
    [JsonPropertyName("hunger")]
    public double Hunger { get; set; } = 80;

    /// <summary>Pet's age in minutes</summary>
    [JsonPropertyName("ageMinutes")]
    public double AgeMinutes { get; set; } = 0;

    /// <summary>Total times fed</summary>
    [JsonPropertyName("timesFed")]
    public int TimesFed { get; set; } = 0;

    /// <summary>Total times played with</summary>
    [JsonPropertyName("timesPlayed")]
    public int TimesPlayed { get; set; } = 0;

    /// <summary>Total times petted</summary>
    [JsonPropertyName("timesPetted")]
    public int TimesPetted { get; set; } = 0;

    /// <summary>Movement speed multiplier</summary>
    [JsonPropertyName("speedMultiplier")]
    public double SpeedMultiplier { get; set; } = 1.0;

    /// <summary>Whether the pet can walk around autonomously</summary>
    [JsonPropertyName("canWalk")]
    public bool CanWalk { get; set; } = true;

    /// <summary>Whether to show particle effects</summary>
    [JsonPropertyName("showParticles")]
    public bool ShowParticles { get; set; } = true;

    /// <summary>Whether to play sounds (reserved for future)</summary>
    [JsonPropertyName("playSounds")]
    public bool PlaySounds { get; set; } = false;

    /// <summary>Pet's primary color (hex)</summary>
    [JsonPropertyName("petColor")]
    public string PetColor { get; set; } = "#FF6B9DFF"; // Cute blue

    /// <summary>Pet's accent/eye color (hex)</summary>
    [JsonPropertyName("accentColor")]
    public string AccentColor { get; set; } = "#FFFFFFFF"; // White

    /// <summary>Last saved window X position</summary>
    [JsonPropertyName("windowX")]
    public double WindowX { get; set; } = 100;

    /// <summary>Last saved window Y position</summary>
    [JsonPropertyName("windowY")]
    public double WindowY { get; set; } = 100;

    /// <summary>Window size</summary>
    [JsonPropertyName("windowSize")]
    public double WindowSize { get; set; } = 120;

    /// <summary>Whether the window position is locked</summary>
    [JsonPropertyName("isLocked")]
    public bool IsLocked { get; set; } = false;

    /// <summary>Last time the pet was saved (for decay calculations)</summary>
    [JsonPropertyName("lastSaveTime")]
    public DateTime LastSaveTime { get; set; } = DateTime.Now;

    /// <summary>Pet's birthday</summary>
    [JsonPropertyName("birthday")]
    public DateTime Birthday { get; set; } = DateTime.Now;
}
