using UnityEngine;
using UnityEngine.UI;

public class ThemedIconDisplay : MonoBehaviour
{
    public enum IconTier { Normal, Elite, Boss, Class }

    Image _icon;
    IconTier _tier = IconTier.Normal;
    float _phase;

    public void Configure(IconTier tier = IconTier.Normal)
    {
        _tier = tier;
        ApplyTheme();
    }

    void Awake()
    {
        _icon = GetComponent<Image>();
        ApplyTheme();
    }

    void Update()
    {
        if (_icon == null) return;
        _phase += Time.deltaTime * ParasiteTowerArtOptimization.MonsterArt.AnimationParams.IdleBreathSpeed;
        float breath = Mathf.Lerp(
            ParasiteTowerArtOptimization.MonsterArt.AnimationParams.IdleBreathMin,
            ParasiteTowerArtOptimization.MonsterArt.AnimationParams.IdleBreathMax,
            0.5f + 0.5f * Mathf.Sin(_phase)
        );
        transform.localScale = Vector3.one * breath;
    }

    void ApplyTheme()
    {
        if (_icon == null) return;
        _icon.color = Color.white;

        var outline = GetComponent<Outline>() ?? gameObject.AddComponent<Outline>();
        Color border = ParasiteTowerColorScheme.UiIconFrameBorder;
        switch (_tier)
        {
            case IconTier.Elite:
                border = ParasiteTowerColorScheme.DangerOrange;
                break;
            case IconTier.Boss:
                border = ParasiteTowerColorScheme.CriticalRed;
                break;
            case IconTier.Class:
                border = ParasiteTowerColorScheme.BioGlowCyan;
                break;
        }
        outline.effectColor = new Color(border.r, border.g, border.b, 0.55f);
        outline.effectDistance = new Vector2(2, 2);
        outline.useGraphicAlpha = true;
    }
}
