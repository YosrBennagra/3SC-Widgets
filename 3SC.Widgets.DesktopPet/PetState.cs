namespace _3SC.Widgets.DesktopPet;

/// <summary>
/// Represents the various behavioral states the pet can be in.
/// </summary>
public enum PetState
{
    /// <summary>Pet is standing still, maybe looking around</summary>
    Idle,

    /// <summary>Pet is walking in a direction</summary>
    Walking,

    /// <summary>Pet is sleeping (restoring energy)</summary>
    Sleeping,

    /// <summary>Pet is eating (restoring hunger)</summary>
    Eating,

    /// <summary>Pet is playing (increases happiness)</summary>
    Playing,

    /// <summary>Pet is being petted by user</summary>
    BeingPetted,

    /// <summary>Pet is excited/celebrating</summary>
    Celebrating,

    /// <summary>Pet is sad (low happiness)</summary>
    Sad,

    /// <summary>Pet is following the mouse cursor</summary>
    FollowingMouse
}

/// <summary>
/// Represents the pet's emotional mood.
/// </summary>
public enum PetMood
{
    Ecstatic,    // > 90 happiness
    Happy,       // 70-90 happiness
    Content,     // 50-70 happiness
    Neutral,     // 30-50 happiness
    Sad,         // 10-30 happiness
    Miserable    // < 10 happiness
}

/// <summary>
/// Direction the pet is facing/moving.
/// </summary>
public enum PetDirection
{
    Left,
    Right
}
