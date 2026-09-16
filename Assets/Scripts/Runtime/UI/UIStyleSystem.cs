using UnityEngine;

/// <summary>
/// UI 样式系统 — 所有颜色委托至 ParasiteTowerColorScheme，保持单一视觉源。
/// </summary>
public class UIStyleSystem : MonoBehaviour
{
    public static UIStyleSystem Instance { get; private set; }

    public static Color PrimaryPurple => ParasiteTowerColorScheme.AbyssPurple;
    public static Color SecondaryCyan => ParasiteTowerColorScheme.HealthGreen;
    public static Color AccentGold => ParasiteTowerColorScheme.AccentGold;
    public static Color DangerRed => ParasiteTowerColorScheme.CriticalRed;

    public static Color TextBright => ParasiteTowerColorScheme.White;
    public static Color TextNormal => ParasiteTowerColorScheme.LightGray;
    public static Color TextDim => ParasiteTowerColorScheme.MidGray;
    public static Color TextDark => ParasiteTowerColorScheme.DarkGray;

    public static Color PanelBgDark => ParasiteTowerColorScheme.UiPanelBg;
    public static Color CardBgDark => ParasiteTowerColorScheme.UiCardBg;
    public static Color CardBgHover => ParasiteTowerColorScheme.UiCardBgHover;
    public static Color BtnBgNormal => ParasiteTowerColorScheme.UiBtnBg;

    public static Color BorderPurple => ParasiteTowerColorScheme.Membrane;
    public static Color GlowPurple => ParasiteTowerColorScheme.BioGlowPurple;
    public static Color GlowCyan => ParasiteTowerColorScheme.BioGlowCyan;

    public static Color IconHealth => ParasiteTowerColorScheme.HealthGreen;
    public static Color IconAttack => ParasiteTowerColorScheme.DangerOrange;
    public static Color IconDefense => ParasiteTowerColorScheme.InfoBlue;
    public static Color IconEvo => ParasiteTowerColorScheme.AccentGold;
    public static Color IconPollution => ParasiteTowerColorScheme.AbyssPurple;

    public const float FadeDuration = 0.3f;
    public const float SlideDuration = 0.4f;
    public const float ScaleDuration = 0.25f;
    public const float PulseInterval = 1.5f;

    public const float RadiusSmall = 8f;
    public const float RadiusMedium = 12f;
    public const float RadiusLarge = 16f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static Color GetLevelColor(int level)
    {
        return level switch
        {
            < 2 => TextNormal,
            < 4 => SecondaryCyan,
            < 6 => AccentGold,
            < 8 => PrimaryPurple,
            _ => DangerRed
        };
    }

    public static Color GetDifficultyColor(int difficulty)
    {
        return difficulty switch
        {
            0 => SecondaryCyan,
            1 => AccentGold,
            2 => PrimaryPurple,
            _ => DangerRed
        };
    }
}
