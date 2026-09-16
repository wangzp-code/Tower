using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 主题 UI 辅助：背景图调色、面板遮罩等。
/// </summary>
public static class ThemeUIHelper
{
    public static Color ArtBackgroundTint(float alpha = 0.82f)
    {
        return new Color(0.78f, 0.72f, 0.92f, alpha);
    }

    public static Color ArtBackgroundDarkOverlay(float alpha = 0.35f)
    {
        var c = ParasiteTowerColorScheme.BioBgDeep;
        c.a = alpha;
        return c;
    }

    public static void ApplyArtBackgroundTint(RawImage img, float alpha = 0.82f)
    {
        if (img == null) return;
        img.color = ArtBackgroundTint(alpha);
    }

    public static void EnsureDarkOverlay(Transform parent, string name = "ThemeDarkOverlay")
    {
        if (parent.Find(name) != null) return;

        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.transform.SetAsLastSibling();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = ArtBackgroundDarkOverlay();
        img.raycastTarget = false;
    }
}
