using UnityEngine;

/// <summary>
/// 原游戏配色方案（基于styles.css）
/// 生物朋克 × 新艺术运动：危险而美丽
/// </summary>
public static class ParasiteTowerColorScheme
{
    // 基础颜色
    public static readonly Color Black = new Color(0f, 0f, 0f);
    public static readonly Color NearBlack = new Color(0.035f, 0.031f, 0.059f); // #0e0c1e
    public static readonly Color DarkGray = new Color(0.102f, 0.086f, 0.188f); // #1a1630
    public static readonly Color MidGray = new Color(0.533f, 0.471f, 0.667f); // #8878aa
    public static readonly Color LightGray = new Color(0.784f, 0.722f, 0.941f); // #c8b8e8
    public static readonly Color White = new Color(0.941f, 0.914f, 1f); // #f0e8ff

    // 语义颜色
    public static readonly Color HealthGreen = new Color(0f, 1f, 0.816f); // #00ffd0
    public static readonly Color WarningYellow = new Color(1f, 0.8f, 0f); // #ffcc00
    public static readonly Color DangerOrange = new Color(1f, 0.42f, 0.208f); // #ff6b35
    public static readonly Color CriticalRed = new Color(1f, 0f, 0.431f); // #ff006e
    public static readonly Color InfoBlue = new Color(0f, 0.784f, 1f); // #00c8ff

    // 职业颜色
    public static readonly Color TitanPrimary = new Color(0.267f, 0.533f, 0.8f); // #4488cc
    public static readonly Color TitanDark = new Color(0.102f, 0.2f, 0.333f); // #1a3355
    public static readonly Color GhostPrimary = new Color(0.8f, 0.267f, 0.8f); // #cc44cc
    public static readonly Color GhostDark = new Color(0.333f, 0.067f, 0.333f); // #551155
    public static readonly Color SwarmPrimary = new Color(0.267f, 0.8f, 0.533f); // #44cc88
    public static readonly Color SwarmDark = new Color(0.102f, 0.267f, 0.2f); // #1a4433
    public static readonly Color BloodPrimary = new Color(0.8f, 0f, 0.133f); // #cc0022
    public static readonly Color BloodDark = new Color(0.2f, 0f, 0.031f); // #330008
    public static readonly Color MechPrimary = new Color(1f, 0.533f, 0f); // #ff8800
    public static readonly Color MechDark = new Color(0.2f, 0.133f, 0f); // #332200

    // 污染渐变
    public static readonly Color PollutionSafe = HealthGreen;
    public static readonly Color PollutionWarning = WarningYellow;
    public static readonly Color PollutionDanger = DangerOrange;
    public static readonly Color PollutionCritical = CriticalRed;

    // UI面板颜色
    public static readonly Color BioBorder = new Color(0f, 1f, 0.816f, 0.35f);
    public static readonly Color BioBorderDim = new Color(0f, 1f, 0.816f, 0.15f);
    public static readonly Color BioHighlight = new Color(0.706f, 0.392f, 1f, 0.15f);
    public static readonly Color Membrane = new Color(0.471f, 0.235f, 0.784f, 0.2f);
    public static readonly Color BioBgPanel = new Color(0.133f, 0.125f, 0.251f, 0.95f);
    public static readonly Color BioBgCard = new Color(0.102f, 0.055f, 0.18f, 0.9f);
    public static readonly Color BioBgDeep = new Color(0.031f, 0.02f, 0.078f);

    // 发光效果
    public static readonly Color BioGlowCyan = new Color(0f, 1f, 0.816f, 0.3f);
    public static readonly Color BioGlowMagenta = new Color(1f, 0f, 0.431f, 0.3f);
    public static readonly Color BioGlowPurple = new Color(0.533f, 0.267f, 1f, 0.3f);
    public static readonly Color GlowOrange = new Color(1f, 0.549f, 0.157f, 0.45f);
    public static readonly Color GlowGreenIntense = new Color(0f, 1f, 0.816f, 0.5f);
    public static readonly Color GlowPurpleIntense = new Color(0.706f, 0.333f, 1f, 0.5f);
    public static readonly Color GlowRedIntense = new Color(1f, 0f, 0.431f, 0.5f);
    public static readonly Color GlowGold = new Color(1f, 0.843f, 0f, 0.4f);

    // 文本发光
    public static readonly Color TextGlowCyan = new Color(0f, 1f, 0.816f, 0.5f);
    public static readonly Color TextGlowOrange = new Color(1f, 0.549f, 0.157f, 0.5f);

    // 其他语义颜色
    public static readonly Color ColorLife = HealthGreen;
    public static readonly Color ColorAttack = DangerOrange;
    public static readonly Color ColorParasite = new Color(0.878f, 0.337f, 0.992f); // #e056fd
    public static readonly Color ColorResource = WarningYellow;
    public static readonly Color ColorInfo = new Color(0.361f, 0.478f, 0.918f); // #5c7aea

    // UI 语义别名（统一 Canvas / System UI 引用）
    public static readonly Color AbyssPurple = new Color(0.65f, 0.25f, 0.95f);
    public static readonly Color AccentGold = new Color(1f, 0.75f, 0.3f);
    public static readonly Color UiPanelBg = new Color(0.07f, 0.05f, 0.12f);
    public static readonly Color UiCardBg = BioBgCard;
    public static readonly Color UiCardBgHover = new Color(0.16f, 0.10f, 0.25f, 0.95f);
    public static readonly Color UiBtnBg = new Color(0.14f, 0.10f, 0.24f, 0.92f);
    public static readonly Color UiBtnBgHover = new Color(0.18f, 0.16f, 0.30f, 0.95f);
    public static readonly Color UiBorder = BioBorder;
    public static readonly Color UiOverlayBg = new Color(0.06f, 0.04f, 0.10f, 0.82f);
    public static readonly Color UiOverlayBgDense = new Color(0.06f, 0.04f, 0.10f, 0.7f);
    public static readonly Color UiSlateText = new Color(0.5f, 0.53f, 0.64f);
    public static readonly Color UiSteelBlue = InfoBlue;
    public static readonly Color UiMutedPurple = new Color(0.6f, 0.55f, 0.7f);
    public static readonly Color UiDarkStat = new Color(0.36f, 0.38f, 0.48f);
    public static readonly Color UiSoftRed = new Color(1f, 0.4f, 0.4f);
    public static readonly Color UiBrightRed = new Color(0.9f, 0.12f, 0.1f);
    public static readonly Color UiSuccessGreen = new Color(0.3f, 0.8f, 0.4f);
    public static readonly Color UiIconFrameBg = DarkGray;
    public static readonly Color UiIconFrameBorder = BioBorder;

    // 获取职业颜色
    public static Color GetClassPrimaryColor(string classId)
    {
        switch (classId.ToLower())
        {
            case "titan": return TitanPrimary;
            case "ghost": return GhostPrimary;
            case "swarm": return SwarmPrimary;
            case "blood": return BloodPrimary;
            case "mech": return MechPrimary;
            default: return LightGray;
        }
    }

    public static Color GetClassDarkColor(string classId)
    {
        switch (classId.ToLower())
        {
            case "titan": return TitanDark;
            case "ghost": return GhostDark;
            case "swarm": return SwarmDark;
            case "blood": return BloodDark;
            case "mech": return MechDark;
            default: return DarkGray;
        }
    }

    // 根据污染值获取颜色
    public static Color GetPollutionColor(float pollution)
    {
        if (pollution < 25f) return PollutionSafe;
        if (pollution < 50f) return PollutionWarning;
        if (pollution < 75f) return PollutionDanger;
        return PollutionCritical;
    }

    // 渐变颜色
    public static Color Lerp(Color from, Color to, float t)
    {
        return Color.Lerp(from, to, Mathf.Clamp01(t));
    }
}
