namespace _3SC.Widgets.AmbientSounds;

public enum SoundType
{
    Rain,
    Thunder,
    Ocean,
    Forest,
    Fire,
    Wind,
    Cafe,
    WhiteNoise,
    BrownNoise,
    PinkNoise
}

public class SoundPreset
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Description { get; set; } = "";
    public SoundType Type { get; set; }
    public string GradientStart { get; set; } = "#1a1a2e";
    public string GradientEnd { get; set; } = "#16213e";
    public double DefaultVolume { get; set; } = 0.5;
}
